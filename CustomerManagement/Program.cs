using Dapr.Client;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDaprClient();

var app = builder.Build();

const string SECRET_STORE_NAME = "daprSecretStore";
const string STRIPE_KEY_NAME = "stripeKey";

app.UseHttpsRedirection();

app.MapPost("/customer", async (
    B2CCustomer newCustomer,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Creating new customer for {newCustomer.Email}.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var customerService = new CustomerService();
        var customerOptions = new CustomerCreateOptions
        {
            Email = newCustomer.Email,
            Name = newCustomer.Name
        };
        var createdCustomer = await customerService.CreateAsync(customerOptions);

        return Results.Ok(createdCustomer);
});

app.MapGet("/customer", async (
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Listing product details.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new CustomerService();
        var options = new CustomerListOptions
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

app.MapGet("/customer/{email}", async (
    string email,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Getting customer details for {email}.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new CustomerService();
        var options = new CustomerSearchOptions
        {
            Query = $"email:\"{email}\"",
        };
        var searchResults = await service.SearchAsync(options);
        var foundCustomer = searchResults.FirstOrDefault();
        if  (foundCustomer == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(foundCustomer);
});

app.Run();

record B2CCustomer(string Name, string Email);