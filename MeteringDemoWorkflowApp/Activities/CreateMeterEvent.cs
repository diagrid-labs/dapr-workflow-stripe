using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace MeteringDemoWorkflowApp
{
    public class CreateMeterEvent : WorkflowActivity<CreateMeterEventInput, CreateMeterEventOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreateMeterEvent(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreateMeterEvent>();
        }

        public override async Task<CreateMeterEventOutput> RunAsync(WorkflowActivityContext context, CreateMeterEventInput input)
        {
            _logger.LogInformation("Create meter event for customer: {CustomerId}.", input.CustomerId);

            try
            {
                await CreateMeterEventTask(input.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create meter event.");
            }

            return new CreateMeterEventOutput(IsSuccess: true);
        }

        private async Task CreateMeterEventTask(string customerId)
        {
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var eventId = Guid.NewGuid().ToString("N");
            var options = new Stripe.Billing.MeterEventCreateOptions
            {
                EventName = "calculation_executed",
                Payload = new Dictionary<string, string>
                {
                    { "value", "10" },
                    { "stripe_customer_id", $"{customerId}" },
                },
                Identifier = $"{eventId}",
                Timestamp = DateTime.UtcNow,
            };
            var service = new Stripe.Billing.MeterEventService();
            service.Create(options);
        }
    }

    public record CreateMeterEventInput(string CustomerId);
    public record CreateMeterEventOutput(bool IsSuccess, string Message = "");
}