using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class MeteredActivityWorkflow : Workflow<MeteredActivityWorkflowInput, MeteredActivityWorkflowOutput>
    {
        public override async Task<MeteredActivityWorkflowOutput> RunAsync(WorkflowContext context, MeteredActivityWorkflowInput input)
        {
            var output = await context.CallActivityAsync<object>(
                input.ActivityName,
                input.ActivityInput);

            var createMeterEventOutput = await context.CallActivityAsync<CreateMeterEventOutput>(
                nameof(CreateMeterEvent),
                new CreateMeterEventInput(input.CustomerId));

            return new MeteredActivityWorkflowOutput(output, IsSuccess: true);
        }
    }

    public record MeteredActivityWorkflowInput(
        string CustomerId,
        string ActivityName,
        object ActivityInput);
    public record MeteredActivityWorkflowOutput(
        object ActivityOutput,
        bool IsSuccess,
        string Message = "");
}