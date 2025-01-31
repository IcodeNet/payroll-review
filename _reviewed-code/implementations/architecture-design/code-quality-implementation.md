[⬅️ Back to Documentation](../../README.md)

# Code Quality Implementation

## Overview
This guide demonstrates how to improve code quality by addressing unit testing, error handling, encapsulation, documentation, and consistent patterns.

## 1. Unit Testing Implementation

```csharp
public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepo;
    private readonly Mock<IValidator<CreateEmployeeCommand>> _mockValidator;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _mockValidator = new Mock<IValidator<CreateEmployeeCommand>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();

        _service = new EmployeeService(
            _mockRepo.Object,
            _mockValidator.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John Doe", "IT");
        var validationResult = new ValidationResult();

        _mockValidator
            .Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.CreateEmployeeAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_WithInvalidData_ReturnsFailure()
    {
        // Arrange
        var command = new CreateEmployeeCommand("", "");
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required")
        });

        _mockValidator
            .Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.CreateEmployeeAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name is required", result.Error);
    }
}
```

## 2. Consistent Error Handling

```csharp
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string title, string message)
        : base(message)
    {
        Title = title;
    }

    public string Title { get; }
}

public class ValidationException : ApplicationException
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Validation Error", "One or more validation errors occurred")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base("Not Found", $"{name} with id {key} was not found")
    {
    }
}

public class DomainException : ApplicationException
{
    public DomainException(string message)
        : base("Domain Rule Violation", message)
    {
    }
}

// Example of consistent error handling in a service
public class EmployeeService
{
    public async Task<Result<EmployeeDto>> GetEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await _repository.GetByIdAsync(id);

            if (employee == null)
                throw new NotFoundException(nameof(Employee), id);

            if (!employee.IsActive)
                throw new DomainException("Inactive employees cannot be retrieved");

            return Result.Success(_mapper.Map<EmployeeDto>(employee));
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            throw;
        }
    }
}
```

## 3. Proper Encapsulation

```csharp
public class Employee : AggregateRoot<Guid>
{
    private readonly List<PaymentHistory> _paymentHistory = new();
    private EmployeeStatus _status;
    private Money _salary;

    protected Employee() { } // For EF Core

    private Employee(
        Guid id,
        EmployeeName name,
        Money salary,
        Department department)
    {
        Id = Guard.Against.Default(id, nameof(id));
        Name = Guard.Against.Null(name, nameof(name));
        _salary = Guard.Against.Null(salary, nameof(salary));
        Department = Guard.Against.Null(department, nameof(department));
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

    public Result UpdateSalary(Money newSalary)
    {
        if (newSalary <= Money.Zero)
            return Result.Failure("Salary must be greater than zero");

        if (_status != EmployeeStatus.Active)
            return Result.Failure("Cannot update salary of inactive employee");

        var oldSalary = _salary;
        _salary = newSalary;

        AddDomainEvent(new EmployeeSalaryUpdatedEvent(Id, oldSalary, newSalary));

        return Result.Success();
    }

    public IReadOnlyCollection<PaymentHistory> PaymentHistory => _paymentHistory.AsReadOnly();
    public Money Salary => _salary;
    public EmployeeStatus Status => _status;
}
```

## 4. Comprehensive Documentation

```csharp
/// <summary>
/// Represents an employee payment record in the system.
/// </summary>
/// <remarks>
/// Payment records are immutable once created and serve as an audit trail
/// for all financial transactions related to an employee.
/// </remarks>
public class PaymentRecord : Entity<Guid>
{
    /// <summary>
    /// Initializes a new payment record.
    /// </summary>
    /// <param name="employeeId">The ID of the employee receiving payment.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="period">The period this payment covers.</param>
    /// <exception cref="ArgumentException">Thrown when amount is negative or period is invalid.</exception>
    public PaymentRecord(Guid employeeId, Money amount, DateRange period)
    {
        Guard.Against.Default(employeeId, nameof(employeeId));
        Guard.Against.Null(amount, nameof(amount));
        Guard.Against.Null(period, nameof(period));

        if (amount <= Money.Zero)
            throw new ArgumentException("Payment amount must be positive", nameof(amount));

        EmployeeId = employeeId;
        Amount = amount;
        Period = period;
        ProcessedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier of the employee receiving the payment.
    /// </summary>
    public Guid EmployeeId { get; }

    /// <summary>
    /// Gets the amount of the payment.
    /// </summary>
    public Money Amount { get; }

    /// <summary>
    /// Gets the period this payment covers.
    /// </summary>
    public DateRange Period { get; }

    /// <summary>
    /// Gets the date when this payment was processed.
    /// </summary>
    public DateTime ProcessedDate { get; }
}
```

## 5. Consistent Patterns

```csharp
// Example of consistent CQRS pattern implementation
public class CreateEmployeeCommand : ICommand<Result<Guid>>
{
    public string FirstName { get; }
    public string LastName { get; }
    public decimal Salary { get; }
    public int DepartmentId { get; }

    public CreateEmployeeCommand(
        string firstName,
        string lastName,
        decimal salary,
        int departmentId)
    {
        FirstName = firstName;
        LastName = lastName;
        Salary = salary;
        DepartmentId = departmentId;
    }
}

public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var name = new EmployeeName(command.FirstName, command.LastName);
        var salary = new Money(command.Salary);

        var employee = await Employee.Create(name, salary);
        if (employee.IsFailure)
            return Result.Failure<Guid>(employee.Error);

        await _repository.AddAsync(employee.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(employee.Value.Id);
    }
}
```

## Key Improvements

1. **Unit Testing**
   - Comprehensive test coverage
   - Clear test structure
   - Proper mocking
   - Test data builders

2. **Error Handling**
   - Consistent exceptions
   - Proper logging
   - Clear error messages
   - Global handling

3. **Encapsulation**
   - Private fields
   - Immutable properties
   - Factory methods
   - Guard clauses

4. **Documentation**
   - XML documentation
   - Code examples
   - Exception documentation
   - Usage guidelines

5. **Consistent Patterns**
   - CQRS implementation
   - Repository pattern
   - Unit of work
   - Domain events