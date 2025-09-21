using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IBankGateway _bankGateway;

    public PaymentsController(PaymentsRepository paymentsRepository, IBankGateway bankGateway)
    {
        _paymentsRepository = paymentsRepository;
        _bankGateway = bankGateway;
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
        if (!ModelState.IsValid)
        {
            return new PostPaymentResponse
            {
                Status = Models.PaymentStatus.Rejected
            };
        }

        var payment = await _bankGateway.SubmitPaymentAsync(request);

        _paymentsRepository.Add(payment);

        return new OkObjectResult(payment);
    }
}