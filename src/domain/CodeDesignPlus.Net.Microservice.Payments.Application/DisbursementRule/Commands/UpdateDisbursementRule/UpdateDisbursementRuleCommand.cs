using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.UpdateDisbursementRule;

/// <summary>
/// Comando para actualizar la regla de comisión de desembolso.
/// Según <see cref="CommissionType"/> se envía la comisión fija o el porcentaje, nunca ambos.
/// </summary>
[DtoGenerator]
public record UpdateDisbursementRuleCommand(
    Guid Id,
    CommissionType CommissionType,
    /// <summary>Comisión fija en unidades mayores con su moneda. Requerido cuando el tipo es Fixed.</summary>
    MoneyInput? FixedCommission,
    /// <summary>Comisión en porcentaje directo (2 para 2%). Requerido cuando el tipo es Percentage.</summary>
    decimal? CommissionPercentage,
    string? Description
) : IRequest;

public class Validator : AbstractValidator<UpdateDisbursementRuleCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();

        When(x => x.CommissionType == CommissionType.Fixed, () =>
        {
            RuleFor(x => x.FixedCommission).NotNull();
            RuleFor(x => x.FixedCommission!.Amount).GreaterThan(0).When(x => x.FixedCommission is not null);
            RuleFor(x => x.FixedCommission!.Currency).NotEmpty().NotNull().Length(3).When(x => x.FixedCommission is not null);
        });

        When(x => x.CommissionType == CommissionType.Percentage, () =>
        {
            RuleFor(x => x.CommissionPercentage).NotNull().GreaterThan(0).LessThanOrEqualTo(100);
        });
    }
}
