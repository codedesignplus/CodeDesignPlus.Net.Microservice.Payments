namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class BanksAggregate(Guid id) : AggregateRootBase(id)
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Code { get; private set; } = null!;


    public BanksAggregate(Guid id, string name,  string description, string code, bool isActive) : this(id)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.BackNameRequired);
        DomainGuard.IsNullOrEmpty(description, Errors.BackDescriptionRequired);
        DomainGuard.IsNullOrEmpty(code, Errors.BackCodeRequired);

        Description = description;
        Code = code;
        IsActive = isActive;

        CreatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public static BanksAggregate Create(Guid id, string name, string description, string code, bool isActive)
    {
        return new BanksAggregate(id, name, description, code, isActive);
    }

    public void Update( string name, string description, bool isActive)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.BackNameRequired);
        DomainGuard.IsNullOrEmpty(description, Errors.BackDescriptionRequired);

        Name = name;
        Description = description;
        IsActive = isActive;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
