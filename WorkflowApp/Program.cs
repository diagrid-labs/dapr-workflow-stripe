using Dapr.Workflow;
using WorkflowApp.SetupStripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDaprClient();
builder.Services.AddDaprWorkflowClient();
builder.Services.AddDaprWorkflow(options =>{
    options.RegisterWorkflow<SetupStripeWorkflow>();
    options.RegisterActivity<CreateCustomer>();
    options.RegisterActivity<CreateProduct>();
    options.RegisterActivity<CreatePrice>();
    options.RegisterActivity<CreateMeter>();
    options.RegisterActivity<CreateSubscription>();
});
var app = builder.Build();

app.MapPost("/setupstripe", async (
    SetupStripeInput input,
    DaprWorkflowClient daprWorkflowClient
    ) => {
        var instanceId = await daprWorkflowClient.ScheduleNewWorkflowAsync(
            nameof(SetupStripeWorkflow),
            input: input);

        return Results.Accepted(instanceId);
});

app.Run();