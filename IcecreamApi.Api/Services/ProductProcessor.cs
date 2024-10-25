using Microsoft.Extensions.Logging;
using IcecreamApi.Api;

namespace IcecreamApi.Services
{
    public class ProductProcessor : IProductProcessor
    {
        private readonly ILogger<ProductProcessor> _logger;

        public ProductProcessor(ILogger<ProductProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<(bool isValid, string? errorMessage)> ValidateProductAsync(Product product)
        {
            _logger.LogInformation("Validating product: {ProductName}", product.Name);

            if (string.IsNullOrWhiteSpace(product.Name))
                return (false, "Product name cannot be empty");

            if (product.Price <= 0)
                return (false, "Price must be greater than 0");

            if (string.IsNullOrWhiteSpace(product.Category))
                return (false, "Category cannot be empty");

            // Simulate async validation (e.g., checking external service)
            await Task.Delay(100);

            return (true, null);
        }

        public async Task<Product> EnrichProductAsync(Product product)
        {
            _logger.LogInformation("Enriching product: {ProductName}", product.Name);

            // Simulate some async enrichment process
            await Task.Delay(100);

            // Add some business logic, for example:
            // If it's a Premium category item, adjust the name to indicate that
            if (product.Category.Equals("Premium", StringComparison.OrdinalIgnoreCase) 
                && !product.Name.Contains("Premium"))
            {
                product.Name = $"Premium {product.Name}";
            }

            return product;
        }
    }
}

