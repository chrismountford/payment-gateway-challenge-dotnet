using System.Text.Json;

using Microsoft.Extensions.Hosting;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

public interface IBankGateway
{
    Task<PostPaymentResponse> SubmitPaymentAsync(SubmitPaymentRequest request);
}

public class BankGateway : IBankGateway
{
    private readonly HttpClient _httpClient;

    public BankGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PostPaymentResponse> SubmitPaymentAsync(SubmitPaymentRequest request)
    {
        var cardNumberStr = request.CardNumber.ToString();

        var body = new BankRequest
        {
            card_number = cardNumberStr,
            expiry_date = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            currency = request.Currency.ToString(),
            amount = request.Amount.ToString(),
            cvv = request.Cvv.ToString(),
        };

        var lastFour = int.Parse(cardNumberStr[^4..]);
        var res = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Declined,
            CardNumberLastFour = lastFour,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
        try
        {
            var bankResponse = await _httpClient.PostAsJsonAsync("http://localhost:8080/payments", body);

            var bankResult = await bankResponse.Content.ReadFromJsonAsync<BankResponse>();

            if (bankResult.Authorized)
            {
                res.Status = PaymentStatus.Authorized;
            }
        }
        catch (Exception ex)
        {
            // Log error
        }

        return res;
    }
}
