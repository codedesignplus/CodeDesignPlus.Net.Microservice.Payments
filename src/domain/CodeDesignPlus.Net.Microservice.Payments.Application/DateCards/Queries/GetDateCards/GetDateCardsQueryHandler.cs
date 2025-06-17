using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.Queries.GetDateCards;

public class GetDateCardsQueryHandler() : IRequestHandler<GetDateCardsQuery, List<DateCardsDto>>
{
    public Task< List<DateCardsDto>> Handle(GetDateCardsQuery request, CancellationToken cancellationToken)
    {
        var now = SystemClock.Instance.GetCurrentInstant().InUtc().ToDateTimeUtc();
        var expirations = GetNextTenYears(now.Month, now.Year);

        var dateCards = expirations.Select(e => new DateCardsDto
        {
            Id = Guid.NewGuid(),
            Month = e.Month,
            Year = e.Year
        }).ToList();

        return Task.FromResult(dateCards);
    }

    public static List<(string Month, string Year)> GetNextTenYears(int currentMonth, int currentYear)
    {
        var result = new List<(string Month, string Year)>();

        for (int year = currentYear; year < currentYear + 10; year++)
        {
            for (int month = 1; month <= 12; month++)
            {
                if (year == currentYear && month < currentMonth)
                    continue;

                result.Add((month.ToString("D2"), year.ToString()));
            }
        }

        return result;
    }
}
