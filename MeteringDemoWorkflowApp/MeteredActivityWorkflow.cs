using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class MeteredActivityWorkflow : Workflow<MeteredActivityWorkflowInput, MeteredActivityWorkflowOutput>
    {
        public override async Task<MeteredActivityWorkflowOutput> RunAsync(WorkflowContext context, MeteredActivityWorkflowInput input)
        {
            var llmOutput = await context.CallActivityAsync<LLMOutput>(
                nameof(CallLLM),
                input.ActivityInput);

            var createMeterEventOutput = await context.CallActivityAsync<CreateMeterEventOutput>(
                nameof(CreateMeterEvent),
                new CreateMeterEventInput(input.CustomerId));

            return new MeteredActivityWorkflowOutput(llmOutput, IsSuccess: true);
        }
    }

    public record MeteredActivityWorkflowInput(
        string CustomerId,
        LLMInput ActivityInput);
    public record MeteredActivityWorkflowOutput(
        LLMOutput ActivityOutput,
        bool IsSuccess,
        string Message = "");
}