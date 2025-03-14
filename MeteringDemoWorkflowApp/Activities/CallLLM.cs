using System;
using Dapr.Client;
using Dapr.Workflow;

namespace MeteringDemoWorkflowApp
{
    public class CallLLM : WorkflowActivity<CallLLMInput, CallLLMOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CallLLM(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CallLLM>();
        }

        public override async Task<CallLLMOutput> RunAsync(WorkflowActivityContext context, CallLLMInput input)
        {
            _logger.LogInformation("Calling LLM with prompt: {Prompt}.", input.Prompt);

            return new CallLLMOutput("Test", IsSuccess: true);
        }
    }

    public record CallLLMInput(string Prompt, string CustomerId);
    public record CallLLMOutput(string Response, bool IsSuccess, string Message = "");
}