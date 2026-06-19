using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.CreatePaymentProviderConfig;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.UpdatePaymentProviderConfig;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.DeletePaymentProviderConfig;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetAllPaymentProviderConfigs;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetPaymentProviderConfigById;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentProviderConfigController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPaymentProviderConfigsQuery(criteria), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentProviderConfigByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentProviderConfigDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<CreatePaymentProviderConfigCommand>(data), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentProviderConfigDto data, CancellationToken cancellationToken)
    {
        var command = mapper.Map<UpdatePaymentProviderConfigCommand>(data) with { Id = id };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePaymentProviderConfigCommand(id), cancellationToken);
        return NoContent();
    }
}
