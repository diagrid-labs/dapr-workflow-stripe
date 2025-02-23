using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace WorkflowApp.SetupStripe
{
    public class CreateProduct : WorkflowActivity<CreateProductInput, CreateProductOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreateProduct(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreateProduct>();
        }
        
        public override async Task<CreateProductOutput> RunAsync(WorkflowActivityContext context, CreateProductInput input)
        {
            _logger.LogInformation("Creating new product: {Name}.", input.Name);
            
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var options = new ProductCreateOptions
            {
                Name = input.Name
            };
            var service = new ProductService();
            var created = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created new product: {Name} {ID}.",
                created.Name,
                created.Id);

            return new CreateProductOutput(created.Id, IsSuccess: true);
        }
    }

    public record CreateProductInput(string Name);
    public record CreateProductOutput(string Id, bool IsSuccess);
}