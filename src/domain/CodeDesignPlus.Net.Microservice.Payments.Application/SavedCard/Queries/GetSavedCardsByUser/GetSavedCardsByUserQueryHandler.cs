using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardsByUser;

public class GetSavedCardsByUserQueryHandler(ISavedCardRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetSavedCardsByUserQuery, List<SavedCardDto>>
{
    public async Task<List<SavedCardDto>> Handle(GetSavedCardsByUserQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var cards = await repository.GetByUserIdAsync(user.IdUser, cancellationToken);

        return mapper.Map<List<SavedCardDto>>(cards);
    }
}
