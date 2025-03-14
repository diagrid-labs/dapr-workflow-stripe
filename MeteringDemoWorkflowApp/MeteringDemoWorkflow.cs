using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class MeteringDemoWorkflow : Workflow<MeteringDemoInput, MeteringDemoOutput>
    {
        public override async Task<MeteringDemoOutput> RunAsync(WorkflowContext context, MeteringDemoInput input)
        {
            var identifyCustomerOutput = await context.CallActivityAsync<IdentifyCustomerOutput>(
                nameof(IdentifyCustomer),
                new IdentifyCustomerInput(input.CustomerEmail));

            var callLLMOutput = await context.CallActivityAsync<CallLLMOutput>(
                nameof(CallLLM),
                new CallLLMInput(input.Prompt, identifyCustomerOutput.CustomerId));

            return new MeteringDemoOutput(IsSuccess: true);
        }
    }

    public record MeteringDemoInput(
        string CustomerEmail,
        string Prompt);
    public record MeteringDemoOutput(
        bool IsSuccess,
        string LastActivity = "",
        string Message = "");
    public record ProductPrice(
        string ProductName,
        string PriceLookupKey,
        decimal UnitAmount);
}