using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using IcecreamApi.Api;
using IcecreamApi.Services;

namespace IcecreamApi.Tests
{
    public class IcecreamApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions;

        public IcecreamApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        [Fact]
        public async Task GetProduct_WithValidId_ReturnsProduct()
        {
            // Arrange
            var client = _factory.CreateClient();
            var productId = 1;

            // Act
            var response = await client.GetAsync($"/api/products/{productId}");
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(content, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            product.Should().NotBeNull();
            product!.Id.Should().Be(productId);
            product.Name.Should().Be("Vanilla");
            product.Category.Should().Be("Classic");
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidProductId = 999;

            // Act
            var response = await client.GetAsync($"/api/products/{invalidProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddProduct_WithValidProduct_ReturnsCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newProduct = new Product
            {
                Id = 3,
                Name = "Chocolate Chip",
                Price = 12.99m,
                Category = "Classic"
            };

            // Create JSON with explicit property names
            var jsonProduct = JsonSerializer.Serialize(newProduct, _jsonOptions);
            Console.WriteLine($"Sending JSON: {jsonProduct}"); // For debugging

            var content = new StringContent(
                jsonProduct,
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PostAsync("/api/products", content);

            // For debugging
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdProduct = JsonSerializer.Deserialize<Product>(responseContent, _jsonOptions);
            createdProduct.Should().NotBeNull();
            createdProduct!.Id.Should().Be(newProduct.Id);
            createdProduct.Name.Should().Be(newProduct.Name);
            createdProduct.Price.Should().Be(newProduct.Price);
            createdProduct.Category.Should().Be(newProduct.Category);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Be($"/api/products/{newProduct.Id}");
        }

        [Fact]
        public async Task AddProduct_WithDuplicateId_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var duplicateProduct = new Product
            {
                Id = 1,
                Name = "Test Ice Cream",
                Price = 9.99m,
                Category = "Classic"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(duplicateProduct),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PostAsync("/api/products", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddProduct_WithInvalidPrice_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidProduct = new Product
            {
                Id = 4,
                Name = "Invalid Ice Cream",
                Price = -10.00m,
                Category = "Classic"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(invalidProduct),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await client.PostAsync("/api/products", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var productId = 2; // Pandan Coconut product

            // Act
            var response = await client.DeleteAsync($"/api/products/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify product is actually deleted
            var getResponse = await client.GetAsync($"/api/products/{productId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidProductId = 999;

            // Act
            var response = await client.DeleteAsync($"/api/products/{invalidProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}