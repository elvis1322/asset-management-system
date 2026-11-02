using System.Net.Http.Json;
using Application.DTO;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class FunFactService : IFunFactService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FunFactService> _logger;
    public FunFactService(HttpClient httpClient, ILogger<FunFactService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetRandomFactAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<CatFactDto>("https://catfact.ninja/fact", cancellationToken);
            return response?.Fact ?? "Nuk u gjet asnjë fakt interesant.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gabim gjatë thirrjes së Fun Fact API.");
            return null;
        }
    }
}