using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardById;

public record GetSavedCardByIdQuery(Guid Id) : IRequest<SavedCardDto>;
