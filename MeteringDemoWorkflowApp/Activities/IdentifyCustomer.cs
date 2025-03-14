using Dapr.Client;
using Dapr.Workflow;
using Stripe;

namespace MeteringDemoWorkflowApp
{
    public class IdentifyCustomer : WorkflowActivity<IdentifyCustomerInput, IdentifyCustomerOutput>
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger _logger;

        public IdentifyCustomer(DaprClient daprClient, ILoggerFactory loggerFactory)
        {
            _daprClient = daprClient;
            _logger = loggerFactory.CreateLogger<CallLLM>();
        }

        public override async Task<IdentifyCustomerOutput> RunAsync(WorkflowActivityContext context, IdentifyCustomerInput input)
        {
            _logger.LogInformation("Identifying customer: {email}.", input.Email);

            var secretDictionary = await _daprClient.GetSecretAsync(
                Constants.SECRET_STORE_NAME,
                Constants.STRIPE_KEY_NAME);
            StripeConfiguration.ApiKey = secretDictionary[Constants.STRIPE_KEY_NAME];

            var service = new CustomerService();
            string customerId = await GetMatchingCustomer(input.Email, service);
            if (customerId == string.Empty)
            {
                return new IdentifyCustomerOutput(customerId, IsSuccess: false, Message: "No matching customer found.");
            }

            return new IdentifyCustomerOutput(customerId, IsSuccess: true);
        }

        private async Task<string> GetMatchingCustomer(string email, CustomerService service)
        {
            string customerId = string.Empty;
            var listOptions = new CustomerListOptions
            {
                Limit = 5,
            };
            var customers = await service.ListAsync(listOptions);
            if (customers == null) return string.Empty;
            
            var matchingCustomer = customers.FirstOrDefault(c => c.Email == email);
            if (matchingCustomer == null) return string.Empty;
            
            return matchingCustomer.Id;
        }
    }

    public record IdentifyCustomerInput(string Email);
    public record IdentifyCustomerOutput(string CustomerId, bool IsSuccess, string Message = "");
}