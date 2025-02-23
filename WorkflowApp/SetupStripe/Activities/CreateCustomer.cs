using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace WorkflowApp.SetupStripe
{
    public class CreateCustomer : WorkflowActivity<CreateCustomerInput, CreateCustomerOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public CreateCustomer(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CreateCustomer>();
        }
        
        public override async Task<CreateCustomerOutput> RunAsync(WorkflowActivityContext context, CreateCustomerInput input)
        {
            _logger.LogInformation("Creating new customer: {Name}.", input.Name);
            
            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var options = new CustomerCreateOptions
            {
                Email = input.Email,
                Name = input.Name
            };
            var service = new CustomerService();
            var created = await service.CreateAsync(options);

            _logger.LogInformation("Successfully created new customer: {Name} {ID}.",
                created.Name,
                created.Id);

            return new CreateCustomerOutput(created.Id, IsSuccess: true);
        }
    }

    public record CreateCustomerInput(string Name, string Email);
    public record CreateCustomerOutput(string Id, bool IsSuccess);
}