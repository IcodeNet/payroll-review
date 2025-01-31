[⬅️ Back to Documentation](../../README.md)

# FullTimeEmployee Implementations

## Implementation Reasoning

### Previous Issues
1. **Holiday Management**
   - Incorrect allowance calculation
   - No carry-over handling
   - Missing adjustments
   - Poor validation

2. **Benefits Handling**
   - No benefits tracking
   - Missing eligibility rules
   - Poor state management
   - Inconsistent application

3. **Contract Management**
   - Poor probation handling
   - Missing contract terms
   - Weak state transitions
   - No term validation

### Changes Made
1. **Holiday System**
   - Proper allowance calculation
   - Carry-over rules
   - Adjustment tracking
   - Strong validation

2. **Benefits System**
   - Benefits tracking
   - Clear eligibility
   - State management
   - Consistent rules

3. **Contract Handling**
   - Probation management
   - Contract terms
   - State machine
   - Term validation

### Benefits
1. **Accuracy**
   - Correct entitlements
   - Clear tracking
   - Better compliance
   - Proper validation

2. **Management**
   - Clear oversight
   - Easy administration
   - Better tracking
   - Policy compliance

3. **Employee Experience**
   - Clear entitlements
   - Easy requests
   - Better visibility
   - Quick responses

```csharp
public class FullTimeEmployee : Employee
{
    private const int STANDARD_HOLIDAY_ALLOWANCE = 25;
    private readonly List<HolidayAdjustment> _holidayAdjustments = new();

    protected FullTimeEmployee() { } // For EF Core

    private FullTimeEmployee(
        Guid id,
        EmployeeName name,
        Money annualPay,
        Department department)
        : base(id, name, annualPay, department, ContractType.FullTime)
    {
    }

    public static Result<FullTimeEmployee> Create(
        EmployeeName name,
        Money annualPay,
        Department department)
    {
        if (annualPay <= Money.Zero)
            return Result.Failure<FullTimeEmployee>("Annual pay must be greater than zero");

        var employee = new FullTimeEmployee(
            Guid.NewGuid(),
            name,
            annualPay,
            department);

        return Result.Success(employee);
    }

    public override int AnnualHolidayAllowance =>
        STANDARD_HOLIDAY_ALLOWANCE + _holidayAdjustments.Sum(a => a.AdjustmentDays);

    public Result AddHolidayAdjustment(int days, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Adjustment reason is required");

        var adjustment = new HolidayAdjustment(days, reason);
        _holidayAdjustments.Add(adjustment);

        AddDomainEvent(new HolidayAllowanceAdjustedEvent(Id, days, reason));

        return Result.Success();
    }

    protected override Result<Money> CalculatePayForPeriod(DateRange period)
    {
        var annualPayPerDay = AnnualPay / 365m;
        return Result.Success(annualPayPerDay * period.TotalDays);
    }
}

public class HolidayAdjustment : ValueObject
{
    public HolidayAdjustment(int days, string reason)
    {
        AdjustmentDays = days;
        Reason = reason;
        AdjustmentDate = DateTime.UtcNow;
    }

    public int AdjustmentDays { get; }
    public string Reason { get; }
    public DateTime AdjustmentDate { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AdjustmentDays;
        yield return Reason;
        yield return AdjustmentDate;
    }
}

public class HolidayAllowanceAdjustedEvent : DomainEvent
{
    public HolidayAllowanceAdjustedEvent(Guid employeeId, int adjustmentDays, string reason)
    {
        EmployeeId = employeeId;
        AdjustmentDays = adjustmentDays;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EmployeeId { get; }
    public int AdjustmentDays { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }
}
```