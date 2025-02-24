using Dapr.Client;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDaprClient();

var app = builder.Build();

const string SECRET_STORE_NAME = "daprSecretStore";
const string STRIPE_KEY_NAME = "stripeKey";

app.UseHttpsRedirection();

app.MapPost("/product", async (
    B2CProduct newProduct,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Creating new customer for {newProduct.Name}.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var service = new ProductService();
        var productOptions = new ProductCreateOptions
        {
            Name = newProduct.Name
        };

        var createdProduct = await service.CreateAsync(productOptions);

        return Results.Ok(createdProduct);
});

app.MapGet("/product", async (
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Listing product details.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new ProductService();
        var options = new ProductListOptions
        {
            Limit = 5,
        };
        var searchResults = await service.ListAsync(options);
        if  (searchResults == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(searchResults);
});

app.MapGet("/product/{name}", async (
    string name,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Getting product details for {name}.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new ProductService();
        var options = new ProductSearchOptions
        {
            Query = $"name:\"{name}\"",
        };
        var searchResults = await service.SearchAsync(options);
        var foundProduct = searchResults.FirstOrDefault();
        if  (foundProduct == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(foundProduct);
});

app.Run();

record B2CProduct(string Id, string Name);