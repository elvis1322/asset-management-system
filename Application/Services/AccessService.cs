using Application.Interfaces;
using Domain.IRepository;
using ErrorOr;

namespace Application.Services;

/// <summary>
/// Shërbim që menaxhon autorizimin, marrjen e të dhënave kontekstuale të përdoruesit korrent
/// (si ID, Roli, Profilizimi si Employee) dhe validimin e qasjes në nivel Departamenti.
/// </summary>
public class AccessService : IAccessService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IEmployeeRepository _employeeRepository;

    public AccessService(
        ICurrentUserService currentUserService,
        IUserService userService,
        IEmployeeRepository employeeRepository)
    {
        _currentUserService = currentUserService;
        _userService = userService;
        _employeeRepository = employeeRepository;
    }
    /// <summary>
    /// Merr ID-në e përdoruesit të identifikuar aktualisht nga JWT / Konteksti i kërkesës.
    /// </summary>
    public ErrorOr<Guid> GetCurrentUserId()
    {
        return _currentUserService.GetUserId();
    }

    /// <summary>
    /// Merr rolin kryesor të përdoruesit aktual.
    /// </summary>
    public ErrorOr<string?> GetCurrentUserRole()
    {
        return _currentUserService.GetRole();
    }
    /// <summary>
    /// Merr në mënyrë asinkrone ID-në e punonjësit (Employee.Id) që është i lidhur me përdoruesin aktual.
    /// </summary>
    public async Task<ErrorOr<Guid>> GetCurrentEmployeeIdAsync(CancellationToken cancellationToken = default)
    {
        // 1. Verifikojmë dhe marrim ID-në e User-it aktual nga konteksti
        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsError) 
            return userIdResult.Errors;
        // 2. Kërkojmë në bazën e të dhënave për profilin e Employee (duke përdorur await për operacionin I/O)
        var employee = await _employeeRepository.GetByUserIdNoTrackingAsync(userIdResult.Value, cancellationToken);
        // 3. Nëse nuk ekziston profili i punonjësit ose ID është bosh, kthejmë gabim 404 (NotFound)
        if (employee is null || employee.Id == Guid.Empty)
        {
            return Error.NotFound("Employee.NotFound", "Nuk u gjet asnjë profil punonjësi i lidhur me përdoruesin tuaj.");
        }
        return employee.Id;
    }
    /// <summary>
    /// Validon dhe kthen ID-në e departamentit të përdoruesit.
    /// Nëse përdoruesi është Admin, kthen `null` (pasi Admin-i ka qasje globale në të gjitha departamentet).
    /// Nëse nuk është Admin, konfirmon që ka profil punonjësi dhe se i është caktuar një departament.
    /// </summary>
    public async Task<ErrorOr<Guid?>> GetValidatedDepartmentIdAsync(CancellationToken cancellationToken = default)
    {
        // 1. Marrja e User ID nga konteksti
        var userIdResult = GetCurrentUserId();
        if (userIdResult.IsError) 
            return userIdResult.Errors;
        // 2. Marrja e të dhënave të plota të User-it nga Database
        var userResult = await _userService.GetByIdAsync(userIdResult.Value, cancellationToken);
        if (userResult.IsError) 
            return userResult.Errors;
        var user = userResult.Value;
        // 3. Nëse është Admin, mundësojmë qasje të pa-kufizuar (null do të thotë nuk ka filtrime sipas departamentit)
        if (user.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
        {
            return (Guid?)null;
        }
        // 4. Për përdoruesit jo-admin, kërkojmë profilin e punonjësit
        var employee = await _employeeRepository.GetByUserIdNoTrackingAsync(userIdResult.Value, cancellationToken);
        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Nuk u gjet profili i punonjësit.");
        }
        // 5. Verifikojmë nëse punonjësi i përket ndonjë departamenti valide
        if (employee.DepartmentId == Guid.Empty)
        {
            return Error.Validation("Employee.NoDepartment", "Përdoruesi nuk është i caktuar në asnjë departament.");
        }
        return employee.DepartmentId;
    }

    /// <summary>
    /// Kontrollon nëse përdoruesi është i autentifikuar.
    /// </summary>
    public bool IsAuthenticated() => _currentUserService.IsAuthenticated();

    /// <summary>
    /// Kontrollon nëse përdoruesi aktual ka rolin Admin.
    /// </summary>
    public bool IsAdmin() => _currentUserService.IsAdmin();

    /// <summary>
    /// Kontrollon nëse përdoruesi aktual ka rolin Manager.
    /// </summary>
    public bool IsManager() => _currentUserService.IsManager();
}