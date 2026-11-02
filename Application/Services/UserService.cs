using ErrorOr;
using Application.DTO.Admin;
using Application.DTO.Auth;
using Application.Models;
using Domain.Entities;
using Domain.IRepository;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Application.Mappers;
namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserAccountService _userAccountService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IEmployeeRepository _employeeRepository;

    public UserService(
        IUserAccountService userAccountService,
        ITransactionRepository transactionRepository,
        ICacheService cacheService,
        ILogger<UserService> logger,
        ICacheInvalidationService cacheInvalidationService,
        IEmployeeRepository employeeRepository)
    {
        _userAccountService = userAccountService;
        _transactionRepository = transactionRepository;
        _cacheService = cacheService;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
        _employeeRepository = employeeRepository;
    }

    public async Task<ErrorOr<PagedResult<UserResponseDto>>> GetUsersPagedAsync(
        PaginationParams paginationParams,
        string? term,
        CancellationToken cancellationToken = default)
    {
        string? sanitizedTerm = null;
        if (!string.IsNullOrWhiteSpace(term))
        {
            sanitizedTerm = term.Trim();
            if (sanitizedTerm.Length > 100)
                sanitizedTerm = sanitizedTerm[..100];
        }
        var (users, totalCount) = await _userAccountService.GetPagedUsersAsync(
            sanitizedTerm,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            cancellationToken);
        var userList = users.ToList();
        var itemDtos = await MapUsersToDtosAsync(userList, cancellationToken);
        return PagedResultExtensions.CreatePagedResult<User, UserResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<UserResponseDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"user_{userId}";
        var cachedUser = await _cacheService.GetAsync<UserResponseDto>(cacheKey, cancellationToken);
        if (cachedUser is not null)
            return cachedUser;
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Përdoruesi me ID {UserId} nuk u gjet.", userId);
            return Error.NotFound("User.NotFound", $"Përdoruesi me ID '{userId}' nuk u gjet.");
        }
        var roles = await _userAccountService.GetUserRolesListAsync(userId);
        var dto = user.MapToDto(roles);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), cancellationToken);
        return dto;
    }

    public async Task<ErrorOr<UserResponseDto>> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default)
    {
        var userExists = await _userAccountService.ExistsByEmailAsync(dto.Email);
        if (userExists)
        {
            _logger.LogWarning("Tentativë krijimi përdoruesi me email ekzistues: {Email}", dto.Email);
            return Error.Conflict("User.EmailExists", $"Emaili '{dto.Email}' është tashmë në përdorim.");
        }
        string role = "Client";
        var createResult = await _userAccountService.CreateUserAsync(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            dto.Genre?.ToUpper() ?? string.Empty,
            role
        );
        if (createResult.IsError)
        {
            _logger.LogWarning("Dështoi krijimi i User për email {Email}", dto.Email);
            return createResult.Errors;
        }
        var user = createResult.Value;
        _logger.LogInformation("U krijua me sukses klienti me UserID: {UserId}", user.Id);
        return user.MapToDto(new List<string> { role });
    }

    public async Task<ErrorOr<UserResponseDto>> UpdateAsync(Guid userId, UpdateAdminUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Tentativë përditësimi për UserID të panjohur: {UserId}", userId);
            return Error.NotFound("User.NotFound", $"Përdoruesi me ID '{userId}' nuk u gjet.");
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var updateIdentityResult = await _userAccountService.UpdateUserIdentityAsync(
                userId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Genre?.ToUpper() ?? string.Empty
            );
            if (updateIdentityResult.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                return updateIdentityResult.Errors;
            }

            var employee = await _employeeRepository.GetByUserIdTrackingAsync(userId, cancellationToken);
            if (employee is not null)
            {
                employee.FirstName = dto.FirstName;
                employee.LastName = dto.LastName;
                employee.Email = dto.Email;
                employee.Gender = dto.Genre?.ToUpper() ?? employee.Gender;
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }

            var currentRoles = await _userAccountService.GetUserRolesListAsync(userId);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
             await _cacheService.RemoveAsync($"user_{userId}", cancellationToken);
            _logger.LogInformation("U përditësua me sukses profili i përdoruesit me UserID: {UserId}", userId);
            var updatedUser = await _userAccountService.GetUserByIdAsync(userId);
            return updatedUser!.MapToDto(currentRoles);
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            _logger.LogError(ex, "Gabim fatal gjatë përditësimit të UserID: {UserId}", userId);
            return Error.Failure("Database.Error", $"Përditësimi i përdoruesit dështoi: {innerMessage}");
        }
    }

    public async Task<ErrorOr<UserResponseDto>> PatchAsync(Guid userId, PatchAdminUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Tentativë patch për UserID të panjohur: {UserId}", userId);
            return Error.NotFound("User.NotFound", $"Përdoruesi me ID '{userId}' nuk u gjet.");
        }
        string firstName = !string.IsNullOrWhiteSpace(dto.FirstName) ? dto.FirstName : user.FirstName;
        string lastName = !string.IsNullOrWhiteSpace(dto.LastName) ? dto.LastName : user.LastName;
        string gender = !string.IsNullOrWhiteSpace(dto.Genre) ? dto.Genre.ToUpper() : user.Gender;
        string email = user.Email!;
        var normalizedGender = GenderHelper.NormalizeGender(dto.Genre);
        if (normalizedGender is null)
        {
            return Error.Validation("User.InvalidGender", "Gjinia e dhënë nuk është e vlefshme. Përdorni Male ose Female.");
        }
        if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userAccountService.GetUserByEmailAsync(dto.Email);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                _logger.LogWarning("Konflikt email gjatë patch për UserID {UserId}: {Email}", userId, dto.Email);
                return Error.Conflict("User.EmailExists", $"Emaili '{dto.Email}' është tashmë në përdorim.");
            }
            email = dto.Email;
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var updateResult = await _userAccountService.UpdateUserIdentityAsync(userId, firstName, lastName, email, gender);
            if (updateResult.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                return updateResult.Errors;
            }

            var employee = await _employeeRepository.GetByUserIdTrackingAsync(userId, cancellationToken);
            if (employee is not null)
            {
                employee.FirstName = firstName;
                employee.LastName = lastName;
                employee.Email = email;
                employee.Gender = normalizedGender;
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.Gender = normalizedGender;
            var currentRoles = await _userAccountService.GetUserRolesListAsync(userId);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheService.RemoveAsync($"user_{userId}", cancellationToken);
            _logger.LogInformation("U bë patch me sukses për profilin e UserID: {UserId}", userId);
            var updatedUser = await _userAccountService.GetUserByIdAsync(userId);
            return updatedUser!.MapToDto(currentRoles);
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë patch për UserID: {UserId}", userId);
            return Error.Failure("Database.Error", "Përditësimi dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Tentativë fshirje për UserID të panjohur: {UserId}", userId);
            return Error.NotFound("User.NotFound", $"Përdoruesi me ID '{userId}' nuk u gjet.");
        }

        var employee = await _employeeRepository.GetByUserIdTrackingAsync(userId, cancellationToken);
        Guid? departmentId = employee?.DepartmentId;

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            if (employee is not null)
            {
                if (employee.Assignments is not null && employee.Assignments.Any(a => a.IsActive))
                {
                    foreach (var assignment in employee.Assignments.Where(a => a.IsActive))
                    {
                        assignment.IsActive = false;
                        assignment.ReturnedDate = DateTime.UtcNow;
                        assignment.Notes += " | Unassigned due to user deletion.";
                        if (assignment.Asset != null && !assignment.Asset.IsShared)
                            assignment.Asset.Status = AssetStatus.InStock;
                    }
                }

                await _employeeRepository.DeleteAsync(employee, cancellationToken);
            }

            var result = await _userAccountService.DeleteUserAsync(userId);
            if (result.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning("Dështoi fshirja e identity user për UserID: {UserId}", userId);
                return result.Errors;
            }

            await _transactionRepository.CommitTransactionAsync(cancellationToken);

            await _cacheService.RemoveAsync($"user_{userId}", cancellationToken);

            await _cacheInvalidationService.InvalidateDashboardCacheAsync([departmentId], cancellationToken);

            _logger.LogInformation("U fshi me sukses përdoruesi me UserID: {UserId}", userId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            _logger.LogError(ex, "Gabim fatal gjatë fshirjes së UserID: {UserId}", userId);
            return Error.Failure("Database.Error", $"Fshirja e përdoruesit dështoi: {innerMessage}");
        }
    }
    private async Task<List<UserResponseDto>> MapUsersToDtosAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
    {
        var result = new List<UserResponseDto>();
        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = await _userAccountService.GetUserRolesListAsync(user.Id);
            result.Add(user.MapToDto(roles));
        }
        return result;
    }
}