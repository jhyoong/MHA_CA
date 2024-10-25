using IcecreamApi.Api;

namespace IcecreamApi.Services
{
    public interface IProductProcessor
    {
        Task<(bool isValid, string? errorMessage)> ValidateProductAsync(Product product);
        Task<Product> EnrichProductAsync(Product product);
    }
}