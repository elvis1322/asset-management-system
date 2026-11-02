using Application.DTO.Employees;
using Application.Interfaces;
using Domain.Entities;
using Domain.IRepository;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Application.Mappers;
namespace Application.Services;

using Application.Models;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserAccountService _userAccountService;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IAccessService _accessService;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IAssetsService _assetService;
    private readonly IDepartmentsService _departmentsService;
    public EmployeeService(
        IEmployeeRepository employeeRepository,
        ITransactionRepository transactionRepository,
        IUserAccountService userAccountService,
        ILogger<EmployeeService> logger,
        IAccessService accessService,
        ICacheInvalidationService cacheInvalidationService,
        IAssetsService assetService,
        IDepartmentsService departmentsService)
    {
        _employeeRepository = employeeRepository;
        _transactionRepository = transactionRepository;
        _userAccountService = userAccountService;
        _logger = logger;
        _accessService = accessService;
        _cacheInvalidationService = cacheInvalidationService;
        _assetService = assetService;
        _departmentsService = departmentsService;
    }

    public async Task<ErrorOr<EmployeeResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation("User.InvalidId", "ID e përdoruesit nuk është e vlefshme.");
        }
        var employee = await _employeeRepository.GetByUserIdNoTrackingAsync(userId, cancellationToken);
        if (employee == null)
        {
            _logger.LogInformation("Nuk u gjet asnjë punonjës për UserId: {UserId}", userId);
            return Error.NotFound("Employee.NotFound", "Punonjësi për këtë përdorues nuk u gjet.");
        }
        var role = await _userAccountService.GetUserRoleAsync(userId);
        var responseDto = employee.EmployeeResponseDto(role);
        return responseDto;
    }

    public async Task<ErrorOr<EmployeeResponseDto>> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdNoTrackingAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError)
            return deptResult.Errors;
        if (deptResult.Value.HasValue && employee.DepartmentId != deptResult.Value.Value)
        {
            return Error.Forbidden("Employee.Unauthorized", "Nuk keni leje të shihni këtë punonjës pasi nuk i përket departamentit tuaj.");
        }
        var role = await _userAccountService.GetUserRoleAsync(employee.UserId);
        return employee.EmployeeResponseDto(role);
    }

    public async Task<ErrorOr<PagedResult<EmployeeResponseDto>>> GetEmployeesPagedAsync(
        PaginationParams paginationParams,
        string? term,
        CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError)
            return deptResult.Errors;
        Guid? effectiveDepartmentId = deptResult.Value;
        string? sanitizedTerm = null;
        if (!string.IsNullOrWhiteSpace(term))
        {
            sanitizedTerm = term.Trim();
            if (sanitizedTerm.Length > 100)
                sanitizedTerm = sanitizedTerm[..100];
        }
        var (employees, totalCount) = await _employeeRepository.GetPagedEmployeesAsync(
            sanitizedTerm,
            effectiveDepartmentId,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            cancellationToken);
        var employeeList = employees.ToList();
        var itemDtos = await MapEmployeesToDtosAsync(employeeList, cancellationToken);
        return PagedResultExtensions.CreatePagedResult<Employee, EmployeeResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<PagedResult<EmployeeByDepartmentResponseDto>>> GetEmployeesByDepartmentAsync(Guid departmentId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _employeeRepository.GetPagedByDepartmentIdAsync(
            departmentId,
             paginationParams.PageNumber,
              paginationParams.PageSize,
               cancellationToken);
        var employeeList = items.ToList();
        var itemDtos = await MapEmployeesToDepartmentDtosAsync(employeeList, cancellationToken);
        return PagedResultExtensions.CreatePagedResult<Employee, EmployeeByDepartmentResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<EmployeeResponseDto>> CreateAsync(
        CreateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _userAccountService.ExistsByEmailAsync(dto.Email);
        if (userExists)
        {
            _logger.LogWarning("Konflikt: përdoruesi '{Email}' ekziston.", dto.Email);
            return Error.Conflict("User.EmailExists", $"Përdoruesi me email '{dto.Email}' ekziston tashmë.");
        }
        if (dto.DepartmentId == Guid.Empty)
        {
            return Error.Validation("Department.Required", "Administratori duhet të caktojë një departament të vlefshëm.");
        }

        var normalizedGender = GenderHelper.NormalizeGender(dto.Gender);
        if (normalizedGender is null)
        {
            return Error.Validation("User.InvalidGender", "Gjinia e dhënë nuk është e vlefshme. Përdorni Male ose Female.");
        }

        var roleToAssign = !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role : "Employee";

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var userResult = await _userAccountService.CreateUserAsync(
                dto.Email,
                dto.Password,
                dto.FirstName,
                dto.LastName,
                dto.Gender,
                roleToAssign
            );

            if (userResult.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning("Dështoi krijimi i User për {Email}.", dto.Email);
                return userResult.Errors;
            }

            var user = userResult.Value;
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Gender = normalizedGender,
                PhoneNumber = dto.PhoneNumber,
                Position = dto.Position,
                DepartmentId = dto.DepartmentId,
                UserId = user.Id,
                IsActive = true,
                JoinedDate = DateTime.UtcNow
            };

            await _employeeRepository.AddAsync(employee, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([employee.DepartmentId], cancellationToken);

            var created = await _employeeRepository.GetByIdTrackingAsync(employee.Id, cancellationToken);
            _logger.LogInformation("U krijua punonjësi {EmpId} ({Email}).", employee.Id, employee.Email);

            return (created ?? employee).EmployeeResponseDto(roleToAssign);
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë krijimit të punonjësit {Email}.", dto.Email);
            return Error.Failure("Database.Error", "Krijimi i punonjësit dështoi.");
        }
    }
    public async Task<ErrorOr<EmployeeResponseDto>> UpdateAsync(
     Guid id,
     UpdateEmployeeDto dto,
     CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdTrackingAsync(id, cancellationToken);
        if (employee is null)
        {
            _logger.LogWarning("Tentativë update — punonjësi {EmpId} nuk u gjet.", id);
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            employee.FirstName = !string.IsNullOrWhiteSpace(dto.FirstName) ? dto.FirstName : employee.FirstName;
            employee.LastName = !string.IsNullOrWhiteSpace(dto.LastName) ? dto.LastName : employee.LastName;
            employee.Email = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : employee.Email;
            employee.PhoneNumber = dto.PhoneNumber ?? employee.PhoneNumber;
            employee.Gender = !string.IsNullOrWhiteSpace(dto.Gender) ? dto.Gender : employee.Gender;

            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            if (employee.UserId != Guid.Empty)
            {
                var identityResult = await _userAccountService.UpdateUserIdentityAsync(
                    employee.UserId,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.Gender
                );

                if (identityResult.IsError)
                {
                    await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning("Dështoi përditësimi i Identity për punonjësin {EmpId}.", id);
                    return identityResult.Errors;
                }
            }

            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            var updated = await _employeeRepository.GetByIdTrackingAsync(id, cancellationToken);
            var updatedRole = await _userAccountService.GetUserRoleAsync(employee.UserId);

            _logger.LogInformation("U përditësua me sukses punonjësi {EmpId}.", id);
            return (updated ?? employee).EmployeeResponseDto(updatedRole);
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë update të punonjësit {EmpId}.", id);
            return Error.Failure("Database.Error", "Përditësimi i punonjësit dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdTrackingAsync(id, cancellationToken);
        if (employee is null)
        {
            _logger.LogWarning("Tentativë fshirje — punonjësi {EmpId} nuk u gjet.", id);
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }

        var deptId = employee.DepartmentId;

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            if (employee.Assignments is not null && employee.Assignments.Any(a => a.IsActive))
            {
                foreach (var assignment in employee.Assignments.Where(a => a.IsActive))
                {
                    assignment.IsActive = false;
                    assignment.ReturnedDate = DateTime.UtcNow;
                    assignment.Notes += " | Unassigned due to employee deletion.";
                    if (assignment.Asset != null && !assignment.Asset.IsShared)
                        assignment.Asset.Status = AssetStatus.InStock;
                }
            }

            // 2. Fshirja e punonjësit nga databaza
            await _employeeRepository.DeleteAsync(employee, cancellationToken);

            // 3. Fshirja e llogarisë së përdoruesit (User Account) nëse ekziston
            if (employee.UserId != Guid.Empty)
            {
                var user = await _userAccountService.GetUserByIdAsync(employee.UserId);
                if (user is not null)
                {
                    var deleteResult = await _userAccountService.DeleteUserAsync(employee.UserId);
                    if (deleteResult.IsError)
                    {
                        await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                        _logger.LogWarning("Dështoi fshirja e User për punonjësin {EmpId}.", id);
                        return Error.Failure("User.DeleteFailed", "Fshirja e përdoruesit dështoi.");
                    }
                }
            }

            await _transactionRepository.CommitTransactionAsync(cancellationToken);

            // 4. Pastrimi i Cache-it
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([deptId], cancellationToken);

            _logger.LogInformation("U fshi punonjësi {EmpId}.", id);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë fshirjes së punonjësit {EmpId}.", id);
            return Error.Failure("Database.Error", $"Fshirja e punonjësit dështoi: {ex.Message}");
        }
    }
    public async Task<ErrorOr<Success>> UpdateDepartmentAsync(
        Guid employeeId,
        Guid newDepartmentId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdTrackingAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            _logger.LogWarning("Tentativë ndryshimi dept — punonjësi {EmpId} nuk u gjet.", employeeId);
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }
        var oldDeptId = employee.DepartmentId;
        if (oldDeptId == newDepartmentId)
        {
            return Result.Success;
        }
        var newDept = await _departmentsService.GetByIdTrackingAsync(newDepartmentId, cancellationToken);
        if (newDept is null)
        {
            return Error.NotFound("Department.NotFound", "Departamenti i ri nuk u gjet.");
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            employee.DepartmentId = newDepartmentId;
            await _employeeRepository.UpdateAsync(employee, cancellationToken);
            var assetResult = await _assetService.UnassignAssetsForEmployeesAsync([employeeId], cancellationToken);
            if (assetResult.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                return assetResult.Errors;
            }
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            Guid? nullableOldDept = oldDeptId == Guid.Empty ? null : oldDeptId;
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([nullableOldDept, newDepartmentId], cancellationToken);
            _logger.LogInformation("Punonjësit {EmpId} iu ndryshua departamenti në {DeptId}.", employeeId, newDepartmentId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë ndryshimit të departamentit për punonjësin {EmpId}.", employeeId);
            return Error.Failure("Database.Error", "Ndryshimi i departamentit dështoi.");
        }
    }
    private async Task<List<EmployeeResponseDto>> MapEmployeesToDtosAsync(
        IEnumerable<Employee> employees,
        CancellationToken cancellationToken = default)
    {
        var result = new List<EmployeeResponseDto>();
        foreach (var emp in employees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = await _userAccountService.GetUserRoleAsync(emp.UserId);
            result.Add(emp.EmployeeResponseDto(role));
        }
        return result;
    }
    private async Task<List<EmployeeByDepartmentResponseDto>> MapEmployeesToDepartmentDtosAsync(
    IEnumerable<Employee> employees,
    CancellationToken cancellationToken = default)
    {
        var result = new List<EmployeeByDepartmentResponseDto>();
        foreach (var emp in employees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var role = await _userAccountService.GetUserRoleAsync(emp.UserId);
            result.Add(emp.EmployeeToDepartmentResponseDto(role));
        }
        return result;
    }

    public async Task<ErrorOr<Success>> DeleteMultipleAsync(
        List<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds == null || !employeeIds.Any())
        {
            return Error.Validation("Employee.EmptyList", "Lista e ID-ve të punonjësve është e zbrazët.");
        }
        var employees = await _employeeRepository.GetByIdsWithTrackingAsync(employeeIds, cancellationToken);
        if (!employees.Any())
        {
            return Error.NotFound("Employee.NotFound", "Asnjë nga punonjësit e kërkuar nuk u gjet.");
        }
        var departmentCounts = await _employeeRepository.GetDepartmentCountsForDeleteAsync(employeeIds, cancellationToken);
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var employee in employees)
            {
                if (employee.Assignments is not null && employee.Assignments.Any(a => a.IsActive))
                {
                    foreach (var assignment in employee.Assignments.Where(a => a.IsActive))
                    {
                        assignment.IsActive = false;
                        assignment.ReturnedDate = DateTime.UtcNow;
                        assignment.Notes += " | Unassigned due to bulk employee deletion.";
                        if (assignment.Asset != null && !assignment.Asset.IsShared)
                            assignment.Asset.Status = AssetStatus.InStock;
                    }
                }
            }
            await _employeeRepository.DeleteMultipleAsync(employeeIds, cancellationToken);
            foreach (var employee in employees)
            {
                if (employee.UserId != Guid.Empty)
                {
                    var user = await _userAccountService.GetUserByIdAsync(employee.UserId);
                    if (user is not null)
                    {
                        var deleteResult = await _userAccountService.DeleteUserAsync(user.Id);
                        if (deleteResult.IsError)
                        {
                            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                            _logger.LogWarning("Dështoi fshirja e User për punonjësin {EmpId}.", employee.Id);
                            return Error.Failure("User.DeleteFailed", "Fshirja e një ose më shumë përdoruesve dështoi.");
                        }
                    }
                }
            }
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            foreach (var departmentId in departmentCounts.Keys)
            {
                await _cacheInvalidationService.InvalidateDashboardCacheAsync([departmentId], cancellationToken);
            }
            _logger.LogInformation("U fshinë me sukses {Count} punonjës në masë.", employees.Count);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë fshirjes masive të punonjësve.");
            return Error.Failure("Database.Error", $"Fshirja masive dështoi: {ex.Message}");
        }
    }

    public async Task<ErrorOr<Success>> UpdateDepartmentMultipleAsync(
        List<Guid> employeeIds,
        Guid newDepartmentId,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds == null || !employeeIds.Any())
        {
            return Error.Validation("Employee.EmptyList", "Lista e punonjësve është e zbrazët.");
        }
        var newDept = await _departmentsService.GetByIdTrackingAsync(newDepartmentId, cancellationToken);
        if (newDept is null)
        {
            return Error.NotFound("Department.NotFound", "Departamenti i ri nuk u gjet.");
        }
        var employees = await _employeeRepository.GetByIdsWithTrackingAsync(employeeIds, cancellationToken);
        if (employees.Count == 0)
        {
            return Error.NotFound("Employee.NotFound", "Asnjë nga punonjësit e kërkuar nuk u gjet.");
        }
        var oldDepartmentCounts = employees
            .Where(e => e.DepartmentId != newDepartmentId)
            .GroupBy(e => e.DepartmentId)
            .ToDictionary(g => g.Key, g => g.Count());
        if (!oldDepartmentCounts.Any())
        {
            return Result.Success;
        }
        var affectedEmployeeIds = employees
            .Where(e => e.DepartmentId != newDepartmentId)
            .Select(e => e.Id)
            .ToList();
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var employee in employees.Where(e => e.DepartmentId != newDepartmentId))
            {
                employee.DepartmentId = newDepartmentId;
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }
            var assetResult = await _assetService.UnassignAssetsForEmployeesAsync(affectedEmployeeIds, cancellationToken);
            if (assetResult.IsError)
            {
                await _transactionRepository.RollbackTransactionAsync(cancellationToken);
                return assetResult.Errors;
            }
            int totalMovedCount = 0;
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            foreach (var oldDeptId in oldDepartmentCounts.Keys)
            {
                Guid? nullableOldDept = oldDeptId == Guid.Empty ? null : oldDeptId;
                await _cacheInvalidationService.InvalidateDashboardCacheAsync([nullableOldDept, newDepartmentId], cancellationToken);
            }
            _logger.LogInformation("U ndryshua departamenti në masë për {Count} punonjës në {NewDept}.", affectedEmployeeIds.Count, newDepartmentId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë ndryshimit masiv të departamentit për punonjësit.");
            return Error.Failure("Database.Error", "Ndryshimi masiv i departamentit dështoi.");
        }
    }

    public async Task<ErrorOr<EmployeeResponseDto>> PromoteUserToEmployeeAsync(
        Guid userId,
        PromoteUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        }
        var existingEmployee = await _employeeRepository.GetByUserIdNoTrackingAsync(userId, cancellationToken);
        if (existingEmployee is not null)
        {
            return Error.Conflict("Employee.AlreadyExists", "Ky përdorues është tashmë i regjistruar si punonjës.");
        }
        var roleResult = await _userAccountService.UpdateUserRolesAsync(userId, new List<string> { "Employee" });
        if (roleResult.IsError)
        {
            return roleResult.Errors;
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var employeeId = Guid.NewGuid();
            var employee = new Employee
            {
                Id = employeeId,
                UserId = userId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Position = "Employee",
                DepartmentId = dto.DepartmentId,
                IsActive = true,
                JoinedDate = DateTime.UtcNow
            };
            await _employeeRepository.AddAsync(employee, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([dto.DepartmentId], cancellationToken);
            var createdEmployee = await _employeeRepository.GetByIdNoTrackingAsync(employeeId, cancellationToken);
            if (createdEmployee == null)
            {
                return Error.Failure("Employee.NotFound", "Punonjësi u krijua por nuk u gjet gjatë leximit.");
            }
            _logger.LogInformation("Përdoruesi {UserId} u promovua me sukses në Employee.", userId);
            return createdEmployee.EmployeeResponseDto("Employee");
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë promovimit të përdoruesit {UserId} në Employee.", userId);
            return Error.Failure("Database.Error", "Promovimi i përdoruesit dështoi.");
        }
    }

    public async Task<ErrorOr<Updated>> UpdateUserRoleAsync(Guid employeeId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdTrackingAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }
        Guid userId = employee.UserId;
        var roleResult = await _userAccountService.UpdateUserRolesAsync(userId, new List<string> { dto.NewRole });
        if (roleResult.IsError)
        {
            return roleResult.Errors;
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            employee.Position = dto.Position;
            employee.IsActive = true;
            await _employeeRepository.UpdateAsync(employee, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([employee.DepartmentId], cancellationToken);
            _logger.LogInformation("Punonjësi {EmployeeId} (User: {UserId}) u përditësua me sukses në rolin/pozitën {NewRole}.", employeeId, userId, dto.NewRole);
            return Result.Updated;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë përditësimit të rolit të punonjësit {EmployeeId}.", employeeId);
            return Error.Failure("Database.Error", "Përditësimi i rolit dështoi.");
        }
    }
}
