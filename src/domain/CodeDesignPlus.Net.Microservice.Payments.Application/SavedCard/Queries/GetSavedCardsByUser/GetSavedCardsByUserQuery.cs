using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardsByUser;

public record GetSavedCardsByUserQuery() : IRequest<List<SavedCardDto>>;
