using Dapr.Workflow;
using MeteringDemoWorkflowApp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprClient();
builder.Services.AddDaprWorkflow(options =>{
    options.RegisterWorkflow<MeteringDemoWorkflow>();
    options.RegisterActivity<IdentifyCustomer>();
    options.RegisterActivity<CallLLM>();
});
var app = builder.Build();

app.MapPost("/run", async (
    MeteringDemoInput input
    ) => {
        
        var workflowClient = app.Services.GetRequiredService<DaprWorkflowClient>();

        var instanceId = await workflowClient.ScheduleNewWorkflowAsync(
            nameof(MeteringDemoWorkflow),
            input: input);

    return Results.Accepted(instanceId);
    });

app.Run();