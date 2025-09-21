using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.IntegrationTests.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly Mock<IBankGateway> _mockBankGateway;

        public CustomWebApplicationFactory(Mock<IBankGateway> mockBankGateway)
        {
            _mockBankGateway = mockBankGateway;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            var paymentsRepository = new PaymentsRepository();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(paymentsRepository);
                services.AddSingleton<IBankGateway>(_ => _mockBankGateway.Object);
            });
        }
    }
}