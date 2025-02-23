using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace WorkflowApp.SetupStripe
{
    public class CreateMeter : WorkflowActivity<CreateMeterInput, CreateMeterOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreateMeter(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreateMeter>();
        }
        
        public override async Task<CreateMeterOutput> RunAsync(WorkflowActivityContext context, CreateMeterInput input)
        {
            _logger.LogInformation("Creating new meter: {Name}.", input.Name);
            
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var options = new Stripe.Billing.MeterCreateOptions
            {
                DisplayName = input.Name,
                EventName = input.EventName,
                DefaultAggregation = new Stripe.Billing.MeterDefaultAggregationOptions
                {
                    Formula = "sum",
                },
                ValueSettings = new Stripe.Billing.MeterValueSettingsOptions
                {
                    EventPayloadKey = "value",
                },
                CustomerMapping = new Stripe.Billing.MeterCustomerMappingOptions
                {
                    Type = "by_id",
                    EventPayloadKey = "stripe_customer_id",
                },
            };
            var service = new Stripe.Billing.MeterService();
            var created = await service.CreateAsync(options);


            _logger.LogInformation("Successfully created new meter: {Name} {ID}.",
                created.DisplayName,
                created.Id);

            return new CreateMeterOutput(created.Id, IsSuccess: true);
        }
    }

    public record CreateMeterInput(string Name, string EventName);
    public record CreateMeterOutput(string Id, bool IsSuccess);
}