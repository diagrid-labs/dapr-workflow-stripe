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
            var options = new SubscriptionCreateOptions
            {
                Customer = input.CustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new() { Price = input.PriceId },
                },
            };

            var created = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created new subscription: {Id}.",
                created.Id);

            return new CreateSubscriptionOutput(created.Id, IsSuccess: true);
        }
    }

    public record CreateSubscriptionInput(string CustomerId, string PriceId);
    public record CreateSubscriptionOutput(string Id, bool IsSuccess);
}