using Dapr.Workflow;

namespace WorkflowApp.SetupStripe
{
    public class SetupStripeWorkflow : Workflow<SetupStripeInput, SetupStripeOutput>
    {
        public override async Task<SetupStripeOutput> RunAsync(WorkflowContext context, SetupStripeInput input)
        {
            var createdCustomer = await context.CallActivityAsync<CreateCustomerOutput>(
                nameof(CreateCustomer),
                new CreateCustomerInput(input.CustomerName, input.CustomerEmail));

            if (!createdCustomer.IsSuccess) return new SetupStripeOutput(IsSuccess: false, LastActivity: nameof(CreateCustomer));
            
            var createdMeter = await context.CallActivityAsync<CreateMeterOutput>(
                nameof(CreateMeter),
                new CreateMeterInput(input.MeterName, input.MeterEventName));

            if (!createdMeter.IsSuccess) return new SetupStripeOutput(IsSuccess: false, LastActivity: nameof(CreateMeter));

            foreach (var productPrice in input.ProductPrices)
            {
                var createdProduct = await context.CallActivityAsync<CreateProductOutput>(
                    nameof(CreateProduct),
                    new CreateProductInput(productPrice.ProductName));
                var createdPrice = await context.CallActivityAsync<CreatePriceOutput>(
                    nameof(CreatePrice),
                    new CreatePriceInput(productPrice.PriceLookupKey, createdMeter.Id, createdProduct.Id, productPrice.UnitAmount));
                var createdSubscription = await context.CallActivityAsync<CreateSubscriptionOutput>(
                    nameof(CreateSubscription),
                    new CreateSubscriptionInput(createdCustomer.Id, createdPrice.Id));
            }

            return new SetupStripeOutput(IsSuccess: true);
        }
    }

    public record SetupStripeInput(
        string CustomerName,
        string CustomerEmail,
        ProductPrice[] ProductPrices,
        string MeterName,
        string MeterEventName);
    public record SetupStripeOutput(bool IsSuccess, string LastActivity = "");
    public record ProductPrice(string ProductName, string PriceLookupKey, decimal UnitAmount);
}