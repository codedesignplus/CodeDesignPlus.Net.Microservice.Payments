using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.CreateBeneficiary;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.UpdateBeneficiary;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.DeactivateBeneficiary;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetAllBeneficiaries;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryById;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryByUserId;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BeneficiaryController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllBeneficiariesQuery(criteria), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBeneficiaryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBeneficiaryByUserIdQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBeneficiaryDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<CreateBeneficiaryCommand>(data), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBeneficiaryDto data, CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateBeneficiaryCommand>(data) with { Id = id };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeactivateBeneficiaryCommand(id), cancellationToken);
        return NoContent();
    }
}
