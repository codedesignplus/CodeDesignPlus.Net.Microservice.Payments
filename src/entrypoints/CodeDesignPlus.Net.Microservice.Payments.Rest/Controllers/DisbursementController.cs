using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetAllDisbursements;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementById;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementsByPayment;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DisbursementController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDisbursementsQuery(criteria), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDisbursementByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetByPayment(Guid paymentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDisbursementsByPaymentQuery(paymentId), cancellationToken);
        return Ok(result);
    }
}
