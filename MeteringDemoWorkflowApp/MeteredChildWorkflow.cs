using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class MeteredChildWorkflow : Workflow<MeteredChildWorkflowInput, MeteredChildWorkflowOutput>
    {
        public override async Task<MeteredChildWorkflowOutput> RunAsync(WorkflowContext context, MeteredChildWorkflowInput input)
        {
            var output = await context.CallActivityAsync<object>(
                input.ActivityName,
                input.ActivityInput);

            var createMeterEventOutput = await context.CallActivityAsync<CreateMeterEventOutput>(
                nameof(CreateMeterEvent),
                new CreateMeterEventInput(input.CustomerId));

            return new MeteredChildWorkflowOutput(output, IsSuccess: true);
        }
    }

    public record MeteredChildWorkflowInput(
        string CustomerId,
        string ActivityName,
        object ActivityInput);
    public record MeteredChildWorkflowOutput(
        object ActivityOutput,
        bool IsSuccess,
        string Message = "");
}