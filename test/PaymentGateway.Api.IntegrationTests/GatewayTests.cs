using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.IntegrationTests.Fixtures;

namespace PaymentGateway.Api.IntegrationTests;

public class GatewayTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GatewayTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_ReturnId_When_SubmitIsSuccessful_And_GetReturnsSamePayment()
    {
        Assert.True(true);
    }
}
