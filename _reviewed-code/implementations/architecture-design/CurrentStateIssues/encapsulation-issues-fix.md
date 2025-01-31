[⬅️ Back to Documentation](../../../README.md)

# Encapsulation Issues - Fixed Implementation

## 1. Employee Entity - Fixed

```csharp
public class Employee : AggregateRoot<Guid>
{
    private readonly List<PaymentHistory> _paymentHistory = new();
    private EmployeeStatus _status;
    private Money _salary;
    private EmployeeName _name;
    private Department _department;

    protected Employee() { } // For EF Core

    private Employee(
        Guid id,
        EmployeeName name,
        Money salary,
        Department department)
    {
        Id = Guard.Against.Default(id, nameof(id));
        _name = Guard.Against.Null(name, nameof(name));
        _salary = Guard.Against.Null(salary, nameof(salary));
        _department = Guard.Against.Null(department, nameof(department));
        _status = EmployeeStatus.Active;
        CreatedDate = DateTime.UtcNow;
    }

    public static Result<Employee> Create(
        EmployeeName name,
        Money salary,
        Department department)
    {
        if (salary <= Money.Zero)
            return Result.Failure<Employee>("Salary must be greater than zero");

        var employee = new Employee(Guid.NewGuid(), name, salary, department);
        employee.AddDomainEvent(new EmployeeCreatedEvent(employee));

        return Result.Success(employee);
    }

    // Public properties with controlled access
    public EmployeeName Name => _name;
    public Department Department => _department;
    public Money Salary => _salary;
    public EmployeeStatus Status => _status;
    public DateTime CreatedDate { get; private set; }
    public IReadOnlyCollection<PaymentHistory> PaymentHistory => _paymentHistory.AsReadOnly();

    public Result AddPayment(PaymentHistory payment)
    {
        Guard.Against.Null(payment, nameof(payment));

        if (_paymentHistory.Any(p => p.Period.Overlaps(payment.Period)))
            return Result.Failure("Payment period overlaps with existing payment");

        _paymentHistory.Add(payment);
        AddDomainEvent(new PaymentAddedEvent(Id, payment));

        return Result.Success();
    }
}
```

## 2. Department Entity - Fixed

```csharp
public class Department : AggregateRoot<int>
{
    private readonly List<Employee> _employees = new();
    private readonly List<Department> _subDepartments = new();
    private DepartmentName _name;
    private bool _isActive;

    protected Department() { } // For EF Core

    private Department(DepartmentName name)
    {
        _name = Guard.Against.Null(name, nameof(name));
        _isActive = true;
        CreatedDate = DateTime.UtcNow;
    }

    public static Result<Department> Create(DepartmentName name)
    {
        var department = new Department(name);
        department.AddDomainEvent(new DepartmentCreatedEvent(department));

        return Result.Success(department);
    }

    // Public properties with controlled access
    public DepartmentName Name => _name;
    public bool IsActive => _isActive;
    public DateTime CreatedDate { get; private set; }
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();
    public IReadOnlyCollection<Department> SubDepartments => _subDepartments.AsReadOnly();

    public Result AddEmployee(Employee employee)
    {
        Guard.Against.Null(employee, nameof(employee));

        if (!_isActive)
            return Result.Failure("Cannot add employee to inactive department");

        if (_employees.Any(e => e.Id == employee.Id))
            return Result.Failure("Employee already in department");

        _employees.Add(employee);
        AddDomainEvent(new EmployeeAddedToDepartmentEvent(Id, employee.Id));

        return Result.Success();
    }
}
```

## 3. PaymentHistory - Fixed

```csharp
public class PaymentHistory : Entity<Guid>
{
    private PaymentStatus _status;
    private readonly Money _amount;
    private readonly DateRange _period;
    private DateTime? _processedDate;

    private PaymentHistory() { } // For EF Core

    private PaymentHistory(
        Employee employee,
        Money amount,
        DateRange period)
    {
        Guard.Against.Null(employee, nameof(employee));
        Guard.Against.Null(amount, nameof(amount));
        Guard.Against.Null(period, nameof(period));

        if (amount <= Money.Zero)
            throw new DomainException("Payment amount must be positive");

        EmployeeId = employee.Id;
        _amount = amount;
        _period = period;
        _status = PaymentStatus.Pending;
        CreatedDate = DateTime.UtcNow;
    }

    public static Result<PaymentHistory> Create(
        Employee employee,
        Money amount,
        DateRange period)
    {
        try
        {
            var payment = new PaymentHistory(employee, amount, period);
            payment.AddDomainEvent(new PaymentCreatedEvent(payment));

            return Result.Success(payment);
        }
        catch (DomainException ex)
        {
            return Result.Failure<PaymentHistory>(ex.Message);
        }
    }

    // Public properties with controlled access
    public Guid EmployeeId { get; private set; }
    public Money Amount => _amount;
    public DateRange Period => _period;
    public PaymentStatus Status => _status;
    public DateTime? ProcessedDate => _processedDate;
    public DateTime CreatedDate { get; private set; }
}
```

## 4. Value Objects - Fixed

```csharp
public class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = Guard.Against.NullOrWhiteSpace(currency, nameof(currency));
    }

    public static Money Zero => new(0, "USD");

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency is required");

        return Result.Success(new Money(amount, currency));
    }

    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot compare money with different currencies");
    }
}
```

## Key Improvements

1. **Proper Encapsulation**
   - Private fields
   - Immutable properties
   - Controlled state changes
   - Protected constructors

2. **Value Objects**
   - Immutable design
   - Self-validation
   - Business rule enforcement
   - Proper equality

3. **Collection Handling**
   - Private collections
   - Read-only access
   - Controlled modification
   - Proper validation

4. **Domain Events**
   - State change tracking
   - Business rule enforcement
   - Audit trail
   - Proper event raising

5. **Factory Methods**
   - Centralized creation
   - Validation rules
   - Error handling
   - Domain events