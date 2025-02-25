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

            var service = new ProductService();
            string productId = await GetMatchingProduct(input.Name, service);

            if (productId == string.Empty)
            {
                try
                {
                    ProductCreateOptions options = GetCreateOptions(input);
                    var created = await service.CreateAsync(options);

                    _logger.LogInformation("Successfully created new product: {Name} {ID}.",
                        created.Name,
                        created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create new product: {Message}.", ex.Message);
                    return new CreateProductOutput(Id: string.Empty, IsSuccess: false, ex.Message);
                }

                
            }

            return new CreateProductOutput(productId, IsSuccess: true);
        }

        private async Task<string> GetMatchingProduct(string productName, ProductService service)
        {
            string productId = string.Empty;
            var listOptions = new ProductListOptions
            {
                Limit = 5,
            };
            var products = await service.ListAsync(listOptions);
            if (products != null)
            {
                var matchingProduct = products.FirstOrDefault(m => m.Name == productName);
                if (matchingProduct != null)
                {
                    productId = matchingProduct.Id;
                    _logger.LogInformation("Product already exists: {Name} {ID}.",
                        matchingProduct.Name,
                        matchingProduct.Id);
                }
            }

            return productId;
        }

        private static ProductCreateOptions GetCreateOptions(CreateProductInput input)
        {
            return new ProductCreateOptions
            {
                Name = input.Name
            };
        }
    }

    public record CreateProductInput(string Name);
    public record CreateProductOutput(string Id, bool IsSuccess, string Message = "");
}