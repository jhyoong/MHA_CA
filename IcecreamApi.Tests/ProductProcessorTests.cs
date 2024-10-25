using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using IcecreamApi.Api;
using IcecreamApi.Services;

namespace IcecreamApi.Tests
{
    public class ProductProcessorTests
    {
        private readonly IProductProcessor _processor;
        private readonly Mock<ILogger<ProductProcessor>> _loggerMock;

        public ProductProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ProductProcessor>>();
            _processor = new ProductProcessor(_loggerMock.Object);
        }

        [Fact]
        public async Task ValidateProduct_WithValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Ice Cream",
                Price = 9.99m,
                Category = "Classic"
            };

            // Act
            var (isValid, errorMessage) = await _processor.ValidateProductAsync(product);

            // Assert
            isValid.Should().BeTrue();
            errorMessage.Should().BeNull();
        }

        [Fact]
        public async Task ValidateProduct_WithInvalidPrice_ReturnsFalse()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Ice Cream",
                Price = -9.99m,
                Category = "Classic"
            };

            // Act
            var (isValid, errorMessage) = await _processor.ValidateProductAsync(product);

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Be("Price must be greater than 0");
        }

        [Fact]
        public async Task EnrichProduct_WithPremiumCategory_AddsPremiumPrefix()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Chocolate Ice Cream",
                Price = 19.99m,
                Category = "Premium"
            };

            // Act
            var enrichedProduct = await _processor.EnrichProductAsync(product);

            // Assert
            enrichedProduct.Name.Should().Be("Premium Chocolate Ice Cream");
        }

        [Fact]
        public async Task EnrichProduct_WithNonPremiumCategory_DoesNotModifyName()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Vanilla Ice Cream",
                Price = 9.99m,
                Category = "Classic"
            };

            // Act
            var enrichedProduct = await _processor.EnrichProductAsync(product);

            // Assert
            enrichedProduct.Name.Should().Be("Vanilla Ice Cream");
        }
    }
}