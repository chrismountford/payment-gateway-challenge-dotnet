using System.Net.Http.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.IntegrationTests.Fixtures;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

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
        var body = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123451,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var submitResponse = await _client.PostAsJsonAsync("/api/Payments/submit", body);
        submitResponse.EnsureSuccessStatusCode();

        var result = await submitResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();
        Assert.NotNull(result);

        var getResponse = await _client.GetAsync($"/api/Payments/{result.Id}");
        getResponse.EnsureSuccessStatusCode();

        var getResult = await getResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        getResult.Should().BeEquivalentTo(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Authorized,
            CardNumberLastFour = 3451,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000
        }, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task Should_ReturnId_When_SubmitIsSuccessful_And_GetReturnsSamePayment_EvenWhenBankRejects()
    {
        var body = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123452,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var submitResponse = await _client.PostAsJsonAsync("/api/Payments/submit", body);
        submitResponse.EnsureSuccessStatusCode();

        var result = await submitResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();
        Assert.NotNull(result);

        var getResponse = await _client.GetAsync($"/api/Payments/{result.Id}");
        getResponse.EnsureSuccessStatusCode();

        var getResult = await getResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        getResult.Should().BeEquivalentTo(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Declined,
            CardNumberLastFour = 3452,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000
        }, options => options.Excluding(x => x.Id));
    }
}
