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
        var body = new BankRequest
        {
            card_number = request.CardNumber,
            expiry_date = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            currency = request.Currency,
            amount = request.Amount,
            cvv = request.Cvv,
        };

        var bankResponse = await _httpClient.PostAsJsonAsync("http://localhost:8080/payments", body);

        var bankResult = await bankResponse.Content.ReadFromJsonAsync<BankResponse>();

        var cardNumberStr = request.CardNumber.ToString();
        var lastFour = int.Parse(cardNumberStr[^4..]);

        return new PostPaymentResponse
        {
            Status = bankResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = lastFour,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
    }
}