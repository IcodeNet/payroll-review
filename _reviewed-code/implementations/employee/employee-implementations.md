[⬅️ Back to Documentation](../../README.md)

# Employee Implementations

## Implementation Reasoning

### Previous Issues
1. **Poor Domain Model**
   - Anemic domain model
   - Public setters everywhere
   - No validation
   - Missing business rules

2. **State Management**
   - Inconsistent state changes
   - No event tracking
   - Missing audit trail
   - Poor encapsulation

3. **Business Logic**
   - Logic scattered in services
   - Missing domain events
   - Weak validation
   - No invariant checks

### Changes Made
1. **Rich Domain Model**
   - Private setters
   - Value objects
   - Strong validation
   - Business rule enforcement

2. **Proper State Management**
   - Controlled state changes
   - Domain events
   - Complete audit trail
   - Proper encapsulation

3. **Domain-Driven Design**
   - Aggregate roots
   - Entity base classes
   - Domain events
   - Business rules in domain

### Benefits
1. **Data Integrity**
   - Consistent state
   - Valid data
   - Clear audit trail
   - Business rule enforcement

2. **Maintainability**
   - Clear boundaries
   - Easy to test
   - Self-documenting
   - Better organization

3. **Domain Logic**
   - Centralized rules
   - Clear validation
   - Event-driven
   - Better traceability

## Base Employee Class
```csharp
public abstract class Employee : AggregateRoot<Guid>
{
    private readonly List<PaymentHistory> _paymentHistory = new();
    private const int DEFAULT_HOLIDAY_ALLOWANCE = 25;

    protected Employee() { } // For EF Core

    protected Employee(
        Guid id,
        EmployeeName name,
        Money annualPay,
        Department department,
        ContractType contractType)
    {
        Id = Guard.Against.Null(id, nameof(id));
        Name = Guard.Against.Null(name, nameof(name));
        AnnualPay = Guard.Against.Null(annualPay, nameof(annualPay));
        Department = Guard.Against.Null(department, nameof(department));
        ContractType = contractType;

        AddDomainEvent(new EmployeeCreatedEvent(this));
    }

    public Guid Id { get; private set; }
    public EmployeeName Name { get; private set; }
    public Department Department { get; private set; }
    public ContractType ContractType { get; private set; }
    public Money AnnualPay { get; private set; }
    public BankDetails BankDetails { get; private set; }
    public abstract int AnnualHolidayAllowance { get; }
    public IReadOnlyCollection<PaymentHistory> PaymentHistory => _paymentHistory.AsReadOnly();

    public Result UpdateName(EmployeeName newName)
    {
        if (newName == null)
            return Result.Failure("Name cannot be null");

        Name = newName;
        AddDomainEvent(new EmployeeNameUpdatedEvent(Id, newName));

        return Result.Success();
    }

    public Result UpdateBankDetails(BankDetails bankDetails)
    {
        var validationResult = BankDetails.Validate(bankDetails);
        if (!validationResult.IsValid)
            return Result.Failure(validationResult.Errors);

        BankDetails = bankDetails;
        AddDomainEvent(new EmployeeBankDetailsUpdatedEvent(Id, bankDetails));

        return Result.Success();
    }

    public Result UpdateSalary(Money newSalary)
    {
        if (newSalary <= Money.Zero)
            return Result.Failure("Salary must be greater than zero");

        var oldSalary = AnnualPay;
        AnnualPay = newSalary;

        AddDomainEvent(new EmployeeSalaryUpdatedEvent(Id, oldSalary, newSalary));

        return Result.Success();
    }

    public Result RecordPayment(DateRange period, Money amount)
    {
        var payment = new PaymentHistory(period, amount);
        _paymentHistory.Add(payment);

        AddDomainEvent(new PaymentRecordedEvent(Id, payment));

        return Result.Success();
    }

    protected abstract Result<Money> CalculatePayForPeriod(DateRange period);
}
```

## Value Objects

### Money Value Object
```csharp
public class Money : ValueObject
{
    public static Money Zero => new(0);

    private Money(decimal amount) => Amount = amount;

    public decimal Amount { get; }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative");

        return Result.Success(new Money(amount));
    }

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);
    public static Money operator *(Money left, decimal multiplier) => new(left.Amount * multiplier);
    public static Money operator /(Money left, decimal divisor) => new(left.Amount / divisor);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}
```

### PaymentHistory Entity
```csharp
public class PaymentHistory : Entity<int>
{
    private PaymentHistory() { } // For EF Core

    public PaymentHistory(DateRange period, Money amount)
    {
        Period = period;
        Amount = amount;
        ProcessedDate = DateTime.UtcNow;
    }

    public DateRange Period { get; private set; }
    public Money Amount { get; private set; }
    public DateTime ProcessedDate { get; private set; }
}
```

### DateRange Value Object
```csharp
public class DateRange : ValueObject
{
    private DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public static Result<DateRange> Create(DateTime start, DateTime end)
    {
        if (end < start)
            return Result.Failure<DateRange>("End date must be after start date");

        return Result.Success(new DateRange(start, end));
    }

    public DateTime Start { get; }
    public DateTime End { get; }
    public int TotalDays => (End - Start).Days + 1;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
```

## Domain Events
```csharp
public class EmployeeCreatedEvent : DomainEvent
{
    public EmployeeCreatedEvent(Employee employee)
    {
        EmployeeId = employee.Id;
        Name = employee.Name.ToString();
        Department = employee.Department.Name.Value;
        ContractType = employee.ContractType;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EmployeeId { get; }
    public string Name { get; }
    public string Department { get; }
    public ContractType ContractType { get; }
    public DateTime OccurredOn { get; }
}

public class EmployeeSalaryUpdatedEvent : DomainEvent
{
    public EmployeeSalaryUpdatedEvent(Guid employeeId, Money oldSalary, Money newSalary)
    {
        EmployeeId = employeeId;
        OldSalary = oldSalary;
        NewSalary = newSalary;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EmployeeId { get; }
    public Money OldSalary { get; }
    public Money NewSalary { get; }
    public DateTime OccurredOn { get; }
}
```

