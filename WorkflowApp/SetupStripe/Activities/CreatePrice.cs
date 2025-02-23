using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace WorkflowApp.SetupStripe
{
    public class CreatePrice : WorkflowActivity<CreatePriceInput, CreatePriceOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreatePrice(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreatePrice>();
        }
        
        public override async Task<CreatePriceOutput> RunAsync(WorkflowActivityContext context, CreatePriceInput input)
        {
            _logger.LogInformation("Creating new price: {LookupKey}.", input.LookupKey);
            
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var service = new PriceService();
            var options = new PriceCreateOptions
            {
                BillingScheme = "per_unit",
                Currency = "usd",
                LookupKey = input.LookupKey,
                Product = input.ProductId,
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month",
                    IntervalCount = 1,
                    Meter = input.MeterId,
                    UsageType = "metered",
                },
                TransformQuantity = new PriceTransformQuantityOptions
                {
                    DivideBy = 10,
                    Round = "up",
                },
                UnitAmountDecimal = input.UnitAmount,
            };

            var created = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created new price: {LookupKey} {ID}.",
                created.LookupKey,
                created.Id);

            return new CreatePriceOutput(created.Id, IsSuccess: true);
        }
    }

    public record CreatePriceInput(string LookupKey, string MeterId, string ProductId, decimal UnitAmount);
    public record CreatePriceOutput(string Id, bool IsSuccess);
}