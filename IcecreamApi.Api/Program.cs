using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using IcecreamApi.Services;
using System.Text.Json.Serialization;

namespace IcecreamApi.Api
{
    public class Product
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 10000.00)]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [Required]
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
    }

    public class Program
    {
        private const string JsonFilePath = "products.json";
        private static readonly object fileLock = new object();
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Method to load products from JSON file
        private static List<Product> LoadProducts(ILogger logger)
        {
            try
            {
                if (!File.Exists(JsonFilePath))
                {
                    // Create initial file with sample data if it doesn't exist
                    var initialProducts = new List<Product>
                    {
                        new Product { Id = 1, Name = "Vanilla", Price = 9.99m, Category = "Classic" },
                        new Product { Id = 2, Name = "Pandan Coconut", Price = 19.99m, Category = "Premium" }
                    };
                    SaveProducts(initialProducts, logger);
                    return initialProducts;
                }

                // Simple lock for thread-safe operations
                lock (fileLock)
                {
                    string jsonContent = File.ReadAllText(JsonFilePath);
                    return JsonSerializer.Deserialize<List<Product>>(jsonContent, jsonOptions) ?? new List<Product>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading products from JSON file");
                return new List<Product>();
            }
        }

        // Method to save products to JSON file
        private static void SaveProducts(List<Product> products, ILogger logger)
        {
            try
            {
                lock (fileLock)
                {
                    string jsonContent = JsonSerializer.Serialize(products, jsonOptions);
                    File.WriteAllText(JsonFilePath, jsonContent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving products to JSON file");
                throw;
            }
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IProductProcessor, ProductProcessor>();

            // Configure JSON serialization options
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.WriteIndented = true;
            });

            // Add logging
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Middleware for request logging
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation(
                    "Request: {Method} {Path} at {Time}",
                    context.Request.Method,
                    context.Request.Path,
                    DateTime.UtcNow
                );
                await next(context);
            });

            // GET endpoint - Get product by ID
            app.MapGet("/api/products/{id}", (int id, ILogger<Program> logger) =>
            {
                logger.LogInformation("Retrieving product with ID: {Id}", id);
                
                var products = LoadProducts(logger);
                var product = products.FirstOrDefault(p => p.Id == id);
                
                if (product == null)
                {
                    logger.LogWarning("Product not found with ID: {Id}", id);
                    return Results.NotFound($"Product with ID {id} not found");
                }

                logger.LogInformation("Product retrieved successfully: {Product}", 
                    JsonSerializer.Serialize(product));
                return Results.Ok(product);
            })
            .WithName("GetProduct")
            .WithOpenApi();

            // POST endpoint - Add new product
            app.MapPost("/api/products", async (
                Product product,
                IProductProcessor processor,
                ILogger<Program> logger) =>
            {
                logger.LogInformation("Attempting to add new product: {Product}", 
                    JsonSerializer.Serialize(product, jsonOptions));

                var products = LoadProducts(logger);

                // Check for duplicate ID
                if (products.Any(p => p.Id == product.Id))
                {
                    logger.LogWarning("Product ID {Id} already exists", product.Id);
                    return Results.BadRequest($"Product with ID {product.Id} already exists");
                }

                // Validate using the processor
                var (isValid, errorMessage) = await processor.ValidateProductAsync(product);
                if (!isValid)
                {
                    logger.LogWarning("Product validation failed: {Error}", errorMessage);
                    return Results.BadRequest(errorMessage);
                }

                // Enrich the product
                product = await processor.EnrichProductAsync(product);

                products.Add(product);
                SaveProducts(products, logger);
                
                logger.LogInformation("Product added successfully");
                return Results.Created($"/api/products/{product.Id}", product);
            })
            .WithName("AddProduct")
            .WithOpenApi();

            // DELETE endpoint - Delete product
            app.MapDelete("/api/products/{id}", (int id, ILogger<Program> logger) =>
            {
                logger.LogInformation("Attempting to delete product with ID: {Id}", id);

                var products = LoadProducts(logger);
                var product = products.FirstOrDefault(p => p.Id == id);
                
                if (product == null)
                {
                    logger.LogWarning("Product not found for deletion. ID: {Id}", id);
                    return Results.NotFound($"Product with ID {id} not found");
                }

                products.Remove(product);
                SaveProducts(products, logger);
                
                logger.LogInformation("Product deleted successfully. ID: {Id}", id);
                return Results.Ok($"Product with ID {id} deleted successfully");
            })
            .WithName("DeleteProduct")
            .WithOpenApi();

            app.Run();
        }
    }
}