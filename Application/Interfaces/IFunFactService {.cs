namespace Application.Interfaces;

public interface IFunFactService
{
    Task<string?> GetRandomFactAsync(CancellationToken cancellationToken = default);
}