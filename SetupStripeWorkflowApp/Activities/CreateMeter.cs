using Dapr.Client;
using Dapr.Workflow;
using Stripe;
using Stripe.Billing;

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

            var service = new MeterService();
            string meterId = await GetMatchingMeter(input.EventName, service);

            if (meterId == string.Empty)
            {
                try
                {
                    MeterCreateOptions createOptions = GetCreateOptions(input);
                    var created = await service.CreateAsync(createOptions);
                    meterId = created.Id;
                    _logger.LogInformation("Successfully created new meter: {Name} {ID}.",
                       created.DisplayName,
                       created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create new meter: {Message}.", ex.Message);
                    return new CreateMeterOutput(Id: string.Empty, IsSuccess: false, ex.Message);
                }
            }

            return new CreateMeterOutput(meterId, IsSuccess: true);
        }

        private async Task<string> GetMatchingMeter(string meterEventName, MeterService service)
        {
            string meterId = string.Empty;
            var listOptions = new MeterListOptions
            {
                Limit = 5,
            };
            var meters = await service.ListAsync(listOptions);
            if (meters != null)
            {
                var matchingMeter = meters.FirstOrDefault(m => m.EventName == meterEventName);
                if (matchingMeter != null)
                {
                    meterId = matchingMeter.Id;
                    _logger.LogInformation("Meter already exists: {Name} {ID}.",
                        matchingMeter.DisplayName,
                        matchingMeter.Id);
                }
            }

            return meterId;
        }

        private static MeterCreateOptions GetCreateOptions(CreateMeterInput input)
        {
            return new MeterCreateOptions
            {
                DisplayName = input.Name,
                EventName = input.EventName,
                DefaultAggregation = new MeterDefaultAggregationOptions
                {
                    Formula = "sum",
                },
                ValueSettings = new MeterValueSettingsOptions
                {
                    EventPayloadKey = "value",
                },
                CustomerMapping = new MeterCustomerMappingOptions
                {
                    Type = "by_id",
                    EventPayloadKey = "stripe_customer_id",
                },
            };
        }

        
    }

    public record CreateMeterInput(string Name, string EventName);
    public record CreateMeterOutput(string Id, bool IsSuccess, string Message = "");
}