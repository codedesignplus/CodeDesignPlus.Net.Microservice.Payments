using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.Queries.GetDateCards;

public record GetDateCardsQuery(int Year) : IRequest<List<DateCardsDto>>;

public class Validation : AbstractValidator<GetDateCardsQuery>
{
    public Validation()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.Now.Year)
            .WithMessage("Year must be between 1900 and the current year.");
    }
}