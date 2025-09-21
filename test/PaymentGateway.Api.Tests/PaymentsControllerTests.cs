using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.IntegrationTests.Fixtures;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly Mock<IBankGateway> _mockBankGateway = new();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(1234567890123456, 1000, 3456)]
    [InlineData(2323232323232323, 9999, 2323)]
    public async Task StoresANewPaymentSuccessfully(long cardNumber, int amount, int lastFour)
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = amount,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();
        ConfigureBankGatewayResponse(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Authorized,
            CardNumberLastFour = lastFour,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = amount
        });

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        result.Should().BeEquivalentTo(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Authorized,
            CardNumberLastFour = lastFour,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = amount
        }, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task ShouldReturnADeclineResponseWhenBankDeclines()
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

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();
        ConfigureBankGatewayResponse(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Declined,
            CardNumberLastFour = 3456,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000
        });

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        result.Should().BeEquivalentTo(new PostPaymentResponse
        {
            Status = Models.PaymentStatus.Declined,
            CardNumberLastFour = 3456,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000
        }, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfCardNumberIsInvalid()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1,
            ExpiryMonth = 2,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.CardNumber)];
        Assert.Contains("Card number is not of the correct length", errors);
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfExpiryMonthIsInvalid()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 55,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.ExpiryMonth)];
        Assert.Contains("Expiry month must be between 1 and 12", errors);
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfExpiryYearIsInvalid()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 2,
            ExpiryYear = 1999,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.ExpiryYear)];
        Assert.Contains("Expiration year must not be in the past", errors);
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfCurrencyIsInvalid()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 1,
            ExpiryYear = 2030,
            Currency = "???",
            Amount = 1000,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.Currency)];
        Assert.Contains("Invalid currency", errors);
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfAmountIsLessThan1()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 1,
            ExpiryYear = 2030,
            Currency = "GBP",
            Amount = 0,
            Cvv = 666,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.Amount)];
        Assert.Contains("Amount must be greater than 0", errors);
    }

    [Fact]
    public async Task FailsToProcessAPaymentIfCvvIsInvalid()
    {
        // Arrange
        var request = new SubmitPaymentRequest
        {
            CardNumber = 1234567890123456,
            ExpiryMonth = 1,
            ExpiryYear = 2030,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 99999,
        };

        var webApplicationFactory = new CustomWebApplicationFactory(_mockBankGateway);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments/submit", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);

        var errors = problemDetails.Errors[nameof(SubmitPaymentRequest.Cvv)];
        Assert.Contains("Cvv must only be 3 or 4 characters", errors);
    }

    public void ConfigureBankGatewayResponse(PostPaymentResponse response)
        {
            _mockBankGateway
                .Setup(m => m.SubmitPaymentAsync(It.IsAny<SubmitPaymentRequest>()))
                .ReturnsAsync(response);
        }
}