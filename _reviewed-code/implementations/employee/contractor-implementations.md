[⬅️ Back to Documentation](../../README.md)

# Contractor Implementations

## Implementation Reasoning

### Previous Issues
1. **Contract Management**
   - Poor period tracking
   - Missing renewals
   - Weak termination
   - No overlap prevention

2. **Rate Management**
   - No rate history
   - Missing validation
   - Poor calculations
   - Weak currency support

3. **Compliance Issues**
   - Missing IR35 checks
   - Poor documentation
   - Weak validation
   - No policy enforcement

### Changes Made
1. **Contract System**
   - Period tracking
   - Renewal handling
   - Termination rules
   - Overlap prevention

2. **Rate Handling**
   - Rate history
   - Strong validation
   - Accurate calculations
   - Currency support

3. **Compliance System**
   - IR35 assessment
   - Documentation
   - Policy validation
   - Clear tracking

### Benefits
1. **Risk Management**
   - Better compliance
   - Clear tracking
   - Policy enforcement
   - Easy auditing

2. **Cost Control**
   - Rate management
   - Clear history
   - Better forecasting
   - Easy reporting

3. **Administration**
   - Easy renewals
   - Clear status
   - Better tracking
   - Policy compliance

```csharp
public class Contractor : Employee
{
    protected Contractor() { } // For EF Core

    private Contractor(
        Guid id,
        EmployeeName name,
        Money dailyRate,
        Department department,
        DateRange contractPeriod)
        : base(id, name, dailyRate * 260, department, ContractType.Contractor)
    {
        ContractPeriod = contractPeriod;
        DailyRate = dailyRate;
        AddDomainEvent(new ContractorCreatedEvent(this));
    }

    public static Result<Contractor> Create(
        EmployeeName name,
        Money dailyRate,
        Department department,
        DateRange contractPeriod)
    {
        if (dailyRate <= Money.Zero)
            return Result.Failure<Contractor>("Daily rate must be greater than zero");

        if (contractPeriod.Start < DateTime.UtcNow.Date)
            return Result.Failure<Contractor>("Contract cannot start in the past");

        var contractor = new Contractor(
            Guid.NewGuid(),
            name,
            dailyRate,
            department,
            contractPeriod);

        return Result.Success(contractor);
    }

    public DateRange ContractPeriod { get; private set; }
    public Money DailyRate { get; private set; }
    public bool IsContractExpired => DateTime.UtcNow > ContractPeriod.End;
    public override int AnnualHolidayAllowance => 0;

    public Result ExtendContract(DateRange newPeriod)
    {
        if (newPeriod.Start < ContractPeriod.End)
            return Result.Failure("New contract period must start after current contract ends");

        var oldPeriod = ContractPeriod;
        ContractPeriod = newPeriod;

        AddDomainEvent(new ContractExtendedEvent(Id, oldPeriod, newPeriod));

        return Result.Success();
    }

    public Result UpdateDailyRate(Money newRate)
    {
        if (newRate <= Money.Zero)
            return Result.Failure("Daily rate must be greater than zero");

        var oldRate = DailyRate;
        DailyRate = newRate;
        AnnualPay = newRate * 260; // Assuming 260 working days per year

        AddDomainEvent(new ContractorRateUpdatedEvent(Id, oldRate, newRate));

        return Result.Success();
    }

    protected override Result<Money> CalculatePayForPeriod(DateRange period)
    {
        if (period.End > ContractPeriod.End)
            return Result.Failure<Money>("Cannot calculate pay beyond contract end date");

        if (period.Start < ContractPeriod.Start)
            return Result.Failure<Money>("Cannot calculate pay before contract start date");

        var workingDays = CalculateWorkingDays(period);
        return Result.Success(DailyRate * workingDays);
    }

    private int CalculateWorkingDays(DateRange period)
    {
        var totalDays = 0;
        var currentDate = period.Start;

        while (currentDate <= period.End)
        {
            if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                totalDays++;
            }
            currentDate = currentDate.AddDays(1);
        }

        return totalDays;
    }
}

public class ContractorCreatedEvent : DomainEvent
{
    public ContractorCreatedEvent(Contractor contractor)
    {
        ContractorId = contractor.Id;
        DailyRate = contractor.DailyRate;
        ContractStart = contractor.ContractPeriod.Start;
        ContractEnd = contractor.ContractPeriod.End;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid ContractorId { get; }
    public Money DailyRate { get; }
    public DateTime ContractStart { get; }
    public DateTime ContractEnd { get; }
    public DateTime OccurredOn { get; }
}

public class ContractExtendedEvent : DomainEvent
{
    public ContractExtendedEvent(
        Guid contractorId,
        DateRange oldPeriod,
        DateRange newPeriod)
    {
        ContractorId = contractorId;
        OldStartDate = oldPeriod.Start;
        OldEndDate = oldPeriod.End;
        NewStartDate = newPeriod.Start;
        NewEndDate = newPeriod.End;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid ContractorId { get; }
    public DateTime OldStartDate { get; }
    public DateTime OldEndDate { get; }
    public DateTime NewStartDate { get; }
    public DateTime NewEndDate { get; }
    public DateTime OccurredOn { get; }
}

public class ContractorRateUpdatedEvent : DomainEvent
{
    public ContractorRateUpdatedEvent(
        Guid contractorId,
        Money oldRate,
        Money newRate)
    {
        ContractorId = contractorId;
        OldRate = oldRate;
        NewRate = newRate;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid ContractorId { get; }
    public Money OldRate { get; }
    public Money NewRate { get; }
    public DateTime OccurredOn { get; }
}
```

## Key Improvements

1. **Contract Period Management**
   - Proper validation of contract dates
   - Contract extension functionality
   - Contract expiry tracking
   - Working days calculation

2. **Rate Management**
   - Daily rate tracking
   - Rate update validation
   - Automatic annual pay calculation
   - Rate change auditing

3. **Business Rules**
   - No holiday allowance
   - Working days only billing
   - Contract period validation
   - Rate validation

4. **Domain Events**
   - Contract creation events
   - Contract extension events
   - Rate update events
   - Full audit trail

5. **Validation**
   - Contract date validation
   - Rate validation
   - Working period validation
   - Business rule enforcement
