using Dapr.Client;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDaprClient();

var app = builder.Build();

const string SECRET_STORE_NAME = "daprSecretStore";
const string STRIPE_KEY_NAME = "stripeKey";

app.UseHttpsRedirection();

app.MapPost("/meter", async (
    B2CMeter newMeter,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Creating new meter.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var options = new Stripe.Billing.MeterCreateOptions
        {
            DisplayName = newMeter.DisplayName,
            EventName = newMeter.EventName,
            DefaultAggregation = new Stripe.Billing.MeterDefaultAggregationOptions
            {
                Formula = "sum",
            },
            ValueSettings = new Stripe.Billing.MeterValueSettingsOptions
            {
                EventPayloadKey = "value",
            },
            CustomerMapping = new Stripe.Billing.MeterCustomerMappingOptions
            {
                Type = "by_id",
                EventPayloadKey = "stripe_customer_id",
            },
        };
        var service = new Stripe.Billing.MeterService();
        var createdMeter = await service.CreateAsync(options);

        return Results.Ok(createdMeter);
});

app.MapGet("/meter", async (
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Listing all meters.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var options = new Stripe.Billing.MeterListOptions
        {
            Limit = 5,
        };
        var service = new Stripe.Billing.MeterService();
        var listResults = await service.ListAsync(options);

        return Results.Ok(listResults);
});

app.MapPost("/price", async (
    B2CPrice newPrice,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Creating new price.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var service = new PriceService();
        var priceOptions = new PriceCreateOptions
        {
            BillingScheme = "per_unit",
            Currency = "usd",
            LookupKey = newPrice.LookupKey,
            Product = newPrice.ProductId,
            Recurring = new PriceRecurringOptions
            {
                Interval = "month",
                IntervalCount = 1,
                Meter = newPrice.MeterId,
                UsageType = "metered",
            },
            TransformQuantity = new PriceTransformQuantityOptions
            {
                DivideBy = 10,
                Round = "up",
            },
            UnitAmountDecimal = newPrice.UnitAmount,
        };

        var createdPrice = await service.CreateAsync(priceOptions);

        return Results.Ok(createdPrice);
});

app.MapGet("/price", async (
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Listing price details.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new PriceService();
        var options = new PriceListOptions
        {
            Limit = 5,
        };
        var listResults = await service.ListAsync(options);
        if  (listResults == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(listResults);
});

app.MapPost("/subscription", async (
    B2CSubscription newSubscription,
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Creating new subscription.");
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];

        var service = new SubscriptionService();
        var subscriptionOptions = new SubscriptionCreateOptions
        {
            Customer = newSubscription.CustomerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions { Price = newSubscription.PriceId },
            },
        };

        var createdSubscription = await service.CreateAsync(subscriptionOptions);

        return Results.Ok(createdSubscription);
});

app.MapGet("/subscription", async (
    DaprClient daprClient
    ) => {
        Console.WriteLine($"Listing subscription details.");;
        
        var keyResult = await daprClient.GetSecretAsync(SECRET_STORE_NAME, STRIPE_KEY_NAME);
        StripeConfiguration.ApiKey = keyResult[STRIPE_KEY_NAME];
        
        var service = new SubscriptionService();
        var options = new SubscriptionListOptions
        {
            Limit = 5,
        };
        var searchResults = await service.ListAsync(options);
        if  (searchResults == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(searchResults);
});

app.Run();

record B2CMeter(string Id, string DisplayName, string EventName);
record B2CPrice(string Id, decimal UnitAmount, string LookupKey, string MeterId, string ProductId);
record B2CSubscription(string Id, string Name, string CustomerId, string PriceId);