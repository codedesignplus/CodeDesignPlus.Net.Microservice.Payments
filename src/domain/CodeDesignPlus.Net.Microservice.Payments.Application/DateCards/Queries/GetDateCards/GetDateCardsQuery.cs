using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.Queries.GetDateCards;

public record GetDateCardsQuery() : IRequest<List<DateCardsDto>>;
