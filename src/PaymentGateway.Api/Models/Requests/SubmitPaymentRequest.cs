using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public class SubmitPaymentRequest
{
    [Range(10000000000000, 9999999999999999999, ErrorMessage = "Card number is not of the correct length")]
    public long CardNumber { get; set; }

    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
    public int ExpiryMonth { get; set; }

    [ExpirationYear]
    public int ExpiryYear { get; set; }

    [AllowedValues(["GBP", "EUR", "USD"], ErrorMessage = "Invalid currency")]
    public string Currency { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }

    [Range(100, 9999, ErrorMessage = "Cvv must only be 3 or 4 characters")]
    public int Cvv { get; set; }
}
