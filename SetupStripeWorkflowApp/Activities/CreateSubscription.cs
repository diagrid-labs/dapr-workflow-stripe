using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace WorkflowApp.SetupStripe
{
    public class CreateSubscription : WorkflowActivity<CreateSubscriptionInput, CreateSubscriptionOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreateSubscription(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreateSubscription>();
        }

        public override async Task<CreateSubscriptionOutput> RunAsync(WorkflowActivityContext context, CreateSubscriptionInput input)
        {
            _logger.LogInformation("Creating new subscription for Customer: {CustomerId}.", input.CustomerId);

            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var service = new SubscriptionService();
            string subscriptionId = await GetMatchingSubscription(input.PriceId, service);

            if (subscriptionId == string.Empty)
            {
                try
                {
                    SubscriptionCreateOptions options = GetCreateOptions(input);
                    var created = await service.CreateAsync(options);
                    subscriptionId = created.Id;
                    _logger.LogInformation("Successfully created new subscription: {Id}.",
                        created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create new subscription: {Message}.", ex.Message);
                    return new CreateSubscriptionOutput(Id: string.Empty, IsSuccess: false, ex.Message);

                }
            }

            return new CreateSubscriptionOutput(subscriptionId, IsSuccess: true);
        }

        private async Task<string> GetMatchingSubscription(string priceId, SubscriptionService service)
        {
            string subscriptionId = string.Empty;
            var listOptions = new SubscriptionListOptions
            {
                Limit = 5,
            };
            var subscriptions = await service.ListAsync(listOptions);
            if (subscriptions != null)
            {
                var matchingSubscription = subscriptions.FirstOrDefault(s => s.Items.Any(i => i.Price.Id == priceId));
                if (matchingSubscription != null)
                {
                    subscriptionId = matchingSubscription.Id;
                    _logger.LogInformation("Subscription already exists: {ID}.",
                        matchingSubscription.Id);
                }
            }

            return subscriptionId;
        }

        private static SubscriptionCreateOptions GetCreateOptions(CreateSubscriptionInput input)
        {
            return new SubscriptionCreateOptions
            {
                Customer = input.CustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new() { Price = input.PriceId },
                },
            };
        }
    }

    public record CreateSubscriptionInput(string CustomerId, string PriceId);
    public record CreateSubscriptionOutput(string Id, bool IsSuccess, string Message = "");
}