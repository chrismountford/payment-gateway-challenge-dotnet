using System.Net;
using System.Text.Json;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class BankGatewayTests
{
    [Fact]
    public async Task ShouldReturnAValidResponseWhenTheBankAuthorisesTheRequest()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var bankGateway = GetGateway(true);

        // Act
        var response = await bankGateway.SubmitPaymentAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, response.Status);
    }

    [Fact]
    public async Task ShouldReturnADeclinedResponseWhenTheBankDeclinesTheRequest()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var bankGateway = GetGateway(false);

        // Act
        var response = await bankGateway.SubmitPaymentAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Declined, response.Status);
    }

    [Fact]
    public async Task ShouldHandleAnUnavailableBankAndReturnDeclined()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var bankGateway = GetGateway(false, false);

        // Act
        var response = await bankGateway.SubmitPaymentAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Declined, response.Status);
    }

    private IBankGateway GetGateway(bool authorized, bool isServiceAvailable = true)
    {
        var content = new
        {
            authorized,
            authorization_code = "0bb07405-6d44-4b50-a14f-7ae0beff13ad",
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(content),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var handlerMock = new Mock<HttpMessageHandler>();

        if (isServiceAvailable)
        {
            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonContent
            });
        }
        else
        {
            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("ServiceUnavailable"));
        }

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://Doesnotmatter.com")
        };

        return new BankGateway(httpClient);
    }
}