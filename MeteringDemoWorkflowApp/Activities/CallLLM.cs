using System;
using Dapr.Client;
using Dapr.Workflow;
using Stripe;

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

            await CreateMeterEvent(input.CustomerId);

            return new CallLLMOutput("Test", IsSuccess: true);
        }

        private async Task CreateMeterEvent(string customerId)
        {
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var eventId = Guid.NewGuid().ToString("N");
            var options = new Stripe.Billing.MeterEventCreateOptions
            {
                EventName = "calculation_execution",
                Payload = new Dictionary<string, string>
                {
                    { "value", "10" },
                    { "stripe_customer_id", $"{customerId}" },
                },
                Identifier = $"{eventId}",
            };
            var service = new Stripe.Billing.MeterEventService();
            service.Create(options);
        }
    }

    public record CallLLMInput(string Prompt, string CustomerId);
    public record CallLLMOutput(string Response, bool IsSuccess, string Message = "");
}