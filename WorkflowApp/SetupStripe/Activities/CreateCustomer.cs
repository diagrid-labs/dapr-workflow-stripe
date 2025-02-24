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

            string customerId = string.Empty;
            var listOptions = new CustomerListOptions
            {
                Limit = 5,
            };
            var service = new CustomerService();
            var customers = await service.ListAsync(listOptions);
            if (customers != null)
            {
                var matchingCustomer = customers.FirstOrDefault(c => c.Email == input.Email);
                if (matchingCustomer != null)
                {
                    customerId = matchingCustomer.Id;
                    _logger.LogInformation("Customer already exists: {Name} {ID}.",
                        matchingCustomer.Name,
                        matchingCustomer.Id);
                }
            }

            if (customerId == string.Empty)
            {
                var createOptions = new CustomerCreateOptions
                {
                    Email = input.Email,
                    Name = input.Name
                };

                try
                {
                    var created = await service.CreateAsync(createOptions);
                    customerId = created.Id;
                    _logger.LogInformation("Successfully created new customer: {Name} {ID}.",
                        created.Name,
                        created.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create new customer: {Message}.", ex.Message);
                    return new CreateCustomerOutput(Id: string.Empty, IsSuccess: false, ex.Message);
                }
            }

            return new CreateCustomerOutput(customerId, IsSuccess: true);
        }
    }

    public record CreateCustomerInput(string Name, string Email);
    public record CreateCustomerOutput(string Id, bool IsSuccess, string Message = "");
}