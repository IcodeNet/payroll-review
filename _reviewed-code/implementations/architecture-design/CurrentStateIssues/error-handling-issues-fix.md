[⬅️ Back to Documentation](../../../README.md)

# Error Handling Issues - Fixed Implementation

## 1. Employee Entity - Fixed

```csharp
public class Employee : AggregateRoot<Guid>
{
    // Fixed: Consistent Result pattern for all operations
    public Result UpdateName(EmployeeName name)
    {
        if (name == null)
            return Result.Failure("Name cannot be null");

        Name = name;
        AddDomainEvent(new EmployeeNameUpdatedEvent(Id, name));
        return Result.Success();
    }

    public Result UpdateSalary(Money amount)
    {
        if (amount <= Money.Zero)
            return Result.Failure("Salary must be greater than zero");

        var oldSalary = Salary;
        Salary = amount;
        AddDomainEvent(new EmployeeSalaryUpdatedEvent(Id, oldSalary, amount));
        return Result.Success();
    }

    public Result UpdateDepartment(Department department)
    {
        if (department == null)
            return Result.Failure("Department cannot be null");

        var oldDepartment = Department;
        Department = department;
        AddDomainEvent(new EmployeeDepartmentChangedEvent(Id, oldDepartment.Id, department.Id));
        return Result.Success();
    }
}
```

## 2. PaymentHistory - Fixed

```csharp
public class PaymentHistory : Entity<Guid>
{
    public Result ProcessPayment()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure($"Cannot process payment in {Status} status");

        Status = PaymentStatus.Processed;
        ProcessedDate = DateTime.UtcNow;
        AddDomainEvent(new PaymentProcessedEvent(Id));
        return Result.Success();
    }

    public Result CancelPayment(string reason)
    {
        if (Status == PaymentStatus.Processed)
            return Result.Failure("Cannot cancel processed payment");

        if (string.IsNullOrEmpty(reason))
            return Result.Failure("Cancellation reason is required");

        Status = PaymentStatus.Cancelled;
        AddDomainEvent(new PaymentCancelledEvent(Id, reason));
        return Result.Success();
    }
}
```

## 3. Controllers - Fixed

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService service,
        ILogger<EmployeeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new ErrorResponse(result.Error));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.Error.Contains("not found")
                ? NotFound(new ErrorResponse(result.Error))
                : BadRequest(new ErrorResponse(result.Error));
    }
}

public class ErrorResponse
{
    public string Message { get; }
    public string[] Details { get; }

    public ErrorResponse(string message, string[] details = null)
    {
        Message = message;
        Details = details ?? Array.Empty<string>();
    }
}
```

## 4. Services - Fixed

```csharp
public class EmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public async Task<Result<EmployeeDto>> CreateAsync(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await Employee.CreateAsync(command);
            if (employee.IsFailure)
                return Result.Failure<EmployeeDto>(employee.Error);

            await _repository.AddAsync(employee.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(_mapper.Map<EmployeeDto>(employee.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return Result.Failure<EmployeeDto>("An error occurred while creating the employee");
        }
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _repository.GetByIdAsync(id, cancellationToken);
            if (employee == null)
                return Result.Failure($"Employee {id} not found");

            var updateResult = await employee.UpdateAsync(command);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return Result.Failure("An error occurred while updating the employee");
        }
    }
}
```

## Key Improvements

1. **Consistent Error Handling**
   - Result pattern throughout
   - Proper error messages
   - Domain events for tracking
   - Proper logging

2. **Structured Error Responses**
   - Consistent format
   - Appropriate status codes
   - Clear error messages
   - Proper error details

3. **Exception Management**
   - Proper try-catch blocks
   - Logging of exceptions
   - Clean error messages
   - Proper error propagation

4. **Business Rule Validation**
   - Clear validation rules
   - Consistent approach
   - Domain-specific errors
   - Proper state checks