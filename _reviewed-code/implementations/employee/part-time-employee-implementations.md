[⬅️ Back to Documentation](../../README.md)

# PartTimeEmployee Implementations

## Implementation Reasoning

### Previous Issues
1. **Hours Management**
   - Poor hours calculation
   - Missing pro-rata
   - Weak validation
   - No minimum/maximum

2. **Benefits Pro-rata**
   - Incorrect calculations
   - Missing validation
   - Poor documentation
   - Inconsistent rules

3. **Schedule Management**
   - No schedule tracking
   - Missing patterns
   - Poor flexibility
   - Weak validation

### Changes Made
1. **Hours System**
   - Accurate calculations
   - Pro-rata handling
   - Strong validation
   - Clear boundaries

2. **Benefits Handling**
   - Pro-rata calculations
   - Clear validation
   - Documented rules
   - Consistent application

3. **Schedule System**
   - Pattern tracking
   - Flexible scheduling
   - Change management
   - Strong validation

### Benefits
1. **Fairness**
   - Correct entitlements
   - Clear calculations
   - Better equity
   - Proper tracking

2. **Compliance**
   - Legal requirements
   - Clear documentation
   - Better tracking
   - Easy auditing

3. **Flexibility**
   - Pattern management
   - Easy changes
   - Better planning
   - Clear tracking

```csharp
public class PartTimeEmployee : Employee
{
    private readonly decimal _workingHoursPercentage;
    private const int FULL_TIME_HOLIDAY = 25;

    protected PartTimeEmployee() { } // For EF Core

    private PartTimeEmployee(
        Guid id,
        EmployeeName name,
        Money annualPay,
        Department department,
        decimal workingHoursPercentage)
        : base(id, name, annualPay, department, ContractType.PartTime)
    {
        if (workingHoursPercentage <= 0 || workingHoursPercentage >= 1)
            throw new DomainException("Working hours percentage must be between 0 and 1");

        _workingHoursPercentage = workingHoursPercentage;
    }

    public static Result<PartTimeEmployee> Create(
        EmployeeName name,
        Money annualPay,
        Department department,
        decimal workingHoursPercentage)
    {
        if (workingHoursPercentage <= 0 || workingHoursPercentage >= 1)
            return Result.Failure<PartTimeEmployee>("Working hours percentage must be between 0 and 1");

        var employee = new PartTimeEmployee(
            Guid.NewGuid(),
            name,
            annualPay,
            department,
            workingHoursPercentage);

        return Result.Success(employee);
    }

    public decimal WorkingHoursPercentage => _workingHoursPercentage;

    public override int AnnualHolidayAllowance =>
        (int)Math.Round(FULL_TIME_HOLIDAY * _workingHoursPercentage);

    protected override Result<Money> CalculatePayForPeriod(DateRange period)
    {
        var fullTimePay = AnnualPay * period.TotalDays / 365m;
        return Result.Success(fullTimePay * _workingHoursPercentage);
    }
}

public class PartTimeEmployeeCreatedEvent : DomainEvent
{
    public PartTimeEmployeeCreatedEvent(
        Guid employeeId,
        decimal workingHoursPercentage)
    {
        EmployeeId = employeeId;
        WorkingHoursPercentage = workingHoursPercentage;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EmployeeId { get; }
    public decimal WorkingHoursPercentage { get; }
    public DateTime OccurredOn { get; }
}