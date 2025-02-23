using Dapr.Client;
using Dapr.Workflow;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDaprClient();
//builder.Services.AddDaprWorkflowClient();
//builder.Services.AddDaprWorkflow(options =>{
    // options.RegisterWorkflow<ValidateOrderWorkflow>();
    // options.RegisterActivity<UpdateInventory>();
    // options.RegisterActivity<UndoUpdateInventory>();
    // options.RegisterActivity<GetShippingCost>();
    // options.RegisterActivity<RegisterShipment>();
//});

var app = builder.Build();

const string SECRET_STORE_NAME = "daprSecretStore";
const string STRIPE_KEY_NAME = "stripeKey";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.MapGet("/customer/{email}", async (
    string email,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Getting customer details for {email}.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        CustomerService customerService = new CustomerService();
        var options = new CustomerSearchOptions
        {
            Query = $"email:'{email}'",
        };
        var service = new CustomerService();
        var searchResults = await service.SearchAsync(options);
        var foundCustomer = searchResults.FirstOrDefault();
        if  (foundCustomer == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(foundCustomer);
});

// app.MapPost("/startWorkflow", async (
//     Order order,
//     DaprWorkflowClient daprWorkflowClient
//     ) => {
//         Console.WriteLine($"Validating order {order.Id} for.");
//         var instanceId = await daprWorkflowClient.ScheduleNewWorkflowAsync(
//             nameof(ValidateOrderWorkflow),
//             instanceId: order.Id,
//             input: order);

//         return Results.Accepted(instanceId);
// });


app.Run();

record B2CCustomer(string Name, string Email);