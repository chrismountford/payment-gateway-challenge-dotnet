using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.IntegrationTests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            var paymentsRepository = new PaymentsRepository();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(paymentsRepository);
            });
        }
    }
}