using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.CreateDisbursementRule;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.UpdateDisbursementRule;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.DeactivateDisbursementRule;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetAllDisbursementRules;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetDisbursementRuleById;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DisbursementRuleController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDisbursementRulesQuery(criteria), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDisbursementRuleByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDisbursementRuleDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<CreateDisbursementRuleCommand>(data), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDisbursementRuleDto data, CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdateDisbursementRuleCommand>(data) with { Id = id };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeactivateDisbursementRuleCommand(id), cancellationToken);
        return NoContent();
    }
}
