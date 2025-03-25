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

            if (identifyCustomerOutput.IsSuccess)
            {
                var childWorkflowOutput = await context.CallChildWorkflowAsync<MeteredActivityWorkflowOutput>(
                    nameof(MeteredActivityWorkflow),
                    new MeteredActivityWorkflowInput(
                        CustomerId: identifyCustomerOutput.CustomerId,
                        ActivityInput: new LLMInput(input.Prompts, identifyCustomerOutput.CustomerId)));

                return new MeteringDemoOutput(Response: childWorkflowOutput.ActivityOutput.Response, IsSuccess: true);
            }

            return new MeteringDemoOutput(Response: null, IsSuccess: false, Message: "Failed to identify customer.");
        }
    }

    public record MeteringDemoInput(
        string CustomerEmail,
        string[] Prompts);
    public record MeteringDemoOutput(
        IEnumerable<string>? Response,
        bool IsSuccess,
        string LastActivity = "",
        string Message = "");
}