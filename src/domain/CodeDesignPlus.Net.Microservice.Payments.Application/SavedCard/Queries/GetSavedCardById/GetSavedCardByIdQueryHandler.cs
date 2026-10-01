using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardById;

public class GetSavedCardByIdQueryHandler(ISavedCardRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetSavedCardByIdQuery, SavedCardDto>
{
    public async Task<SavedCardDto> Handle(GetSavedCardByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<SavedCardAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.SavedCardNotFound);

        // Una tarjeta ajena responde igual que una que no existe: no se revela que existe (pendings/180).
        ApplicationGuard.IsTrue(aggregate.UserId != user.IdUser, Errors.SavedCardNotFound);

        return mapper.Map<SavedCardDto>(aggregate);
    }
}
