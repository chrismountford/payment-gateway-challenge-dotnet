using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public class SubmitPaymentRequest : IValidatableObject
{
    public ulong CardNumber { get; set; }

    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    public string Currency { get; set; }

    public int Amount { get; set; }

    public int Cvv { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CardNumber < 10000000000000 || CardNumber > 9999999999999999999)
        {
            yield return new ValidationResult("Card number is not of the correct length", [nameof(CardNumber)]);

        }

        if (ExpiryYear < DateTime.UtcNow.Year ||
            (ExpiryYear == DateTime.UtcNow.Year && ExpiryMonth < DateTime.UtcNow.Month))
        {
            yield return new ValidationResult(
                "Expiry date must be in the future",
                [nameof(ExpiryMonth), nameof(ExpiryYear)]
            );
        }

        var allowedCurrencies = new[] { "GBP", "EUR", "USD" };
        if (!allowedCurrencies.Contains(Currency))
        {
            yield return new ValidationResult("Invalid currency", [nameof(Currency)]);
        }

        if (Amount < 1)
        {
            yield return new ValidationResult("Amount must be greater than 0", [nameof(Amount)]);
        }

        if (Cvv < 100 || Cvv > 9999)
        {
            yield return new ValidationResult("Cvv must only be 3 or 4 characters", [nameof(Cvv)]);
        }
    }
}
