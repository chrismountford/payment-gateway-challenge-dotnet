using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment != null)
        {
            return new OkObjectResult(payment);
        }

        return new NotFoundObjectResult("Payment does not exist");
    }

    [HttpPost("submit")]
    public async Task<ActionResult<PostPaymentResponse>> SubmitPaymentAsync([FromBody] SubmitPaymentRequest request)
    {
        var cardNumberStr = request.CardNumber.ToString();
        var lastFour = int.Parse(cardNumberStr[^4..]);

        // We need to send to the bank
        
        var payment = new PostPaymentResponse
        {
            Id = new Guid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = lastFour,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        _paymentsRepository.Add(payment);

        return new OkObjectResult(payment);
    }
}