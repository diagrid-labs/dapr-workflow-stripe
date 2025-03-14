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
            string priceId = await GetMatchingPrice(input.LookupKey, service);

            if (priceId == string.Empty)
            {
                try
                {
                    PriceCreateOptions createOptions = GetCreateOptions(input);
                    var created = await service.CreateAsync(createOptions);
                    priceId = created.Id;
                    _logger.LogInformation("Successfully created new price: {LookupKey} {ID}.",
                        created.LookupKey,
                        created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create new price: {Message}.", ex.Message);
                    return new CreatePriceOutput(Id: string.Empty, IsSuccess: false, ex.Message);
                }
            }

            return new CreatePriceOutput(priceId, IsSuccess: true);
        }

        private async Task<string> GetMatchingPrice(string lookupKey, PriceService service)
        {
            string priceId = string.Empty;
            var listOptions = new PriceListOptions
            {
                Limit = 5,
            };
            var prices = await service.ListAsync(listOptions);
            if (prices != null)
            {
                var matchingPrice = prices.FirstOrDefault(p => p.LookupKey == lookupKey);
                if (matchingPrice != null)
                {
                    priceId = matchingPrice.Id;
                    _logger.LogInformation("Price already exists: {LookupKey} {ID}.",
                        matchingPrice.LookupKey,
                        matchingPrice.Id);
                }
            }

            return priceId;
        }

        private static PriceCreateOptions GetCreateOptions(CreatePriceInput input)
        {
            return new PriceCreateOptions
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
        }
    }
    public record CreatePriceInput(string LookupKey, string MeterId, string ProductId, decimal UnitAmount);
    public record CreatePriceOutput(string Id, bool IsSuccess, string Message = "");
}