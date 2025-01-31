[⬅️ Back to Documentation](../../../README.md)

# Architecture Issues

> 🔧 **Implementation Fix**: See [Architecture Issues Fix](../CurrentStateIssues/architecture-issues-fix.md) for the solution.

## 1. Missing Service Layer

```csharp
// Issue: Controllers directly using repositories
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _repository;

    // Issue: Direct repository dependency
    public EmployeeController(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        // Issue: Business logic in controller
        if (dto.Salary < 0)
            return BadRequest("Salary must be positive");

        // Issue: Direct entity manipulation
        var employee = new Employee
        {
            Name = dto.Name,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(employee);
        return Ok(employee);
    }
}

// Issue: Repository directly accessed by controllers
public class PayrollController : ControllerBase
{
    private readonly IPayrollRepository _repository;

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayroll(int departmentId)
    {
        // Issue: Complex business logic in controller
        var employees = await _repository.GetByDepartmentAsync(departmentId);
        foreach (var employee in employees)
        {
            // Business logic scattered in controller
            var salary = CalculateSalary(employee);
            var payment = new PaymentHistory
            {
                EmployeeId = employee.Id,
                Amount = salary,
                ProcessedDate = DateTime.UtcNow
            };
            await _repository.AddPaymentAsync(payment);
        }

        return Ok();
    }
}
```

## 2. Poor Dependency Injection

```csharp
// Issue: Direct instantiation of dependencies
public class EmployeeService
{
    // Issue: Hard-coded dependencies
    private readonly SqlConnection _connection =
        new("Server=.;Database=Payroll;");

    // Issue: Static logger
    private static readonly ILogger _logger =
        LogManager.GetCurrentClassLogger();

    // Issue: Direct instantiation
    private readonly EmailService _emailService = new();

    public async Task ProcessPayment(Guid employeeId, decimal amount)
    {
        try
        {
            // Direct database access
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO Payments...";
            await command.ExecuteNonQueryAsync();

            // Direct email service usage
            await _emailService.SendPaymentNotification(employeeId, amount);
        }
        catch (Exception ex)
        {
            // Static logger usage
            _logger.Error(ex, "Payment processing failed");
            throw;
        }
    }
}

// Issue: Service locator anti-pattern
public class PayrollProcessor
{
    public async Task ProcessPayroll()
    {
        // Issue: Service locator usage
        var scope = ServiceLocator.Current.CreateScope();
        var repository = scope.GetService<IEmployeeRepository>();
        var emailService = scope.GetService<IEmailService>();

        // Process payroll...
    }
}
```

## 3. Missing Validation Strategy

```csharp
// Issue: Inconsistent validation
public class EmployeeController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        // Issue: Manual validation
        if (string.IsNullOrEmpty(dto.Name))
            return BadRequest("Name is required");

        if (dto.Salary < 0)
            return BadRequest("Salary must be positive");

        // Create employee...
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        // Issue: Different validation approach
        try
        {
            ValidateEmployee(dto);
            // Update employee...
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Issue: Validation logic scattered
    private void ValidateEmployee(UpdateEmployeeDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(dto.Name))
            errors.Add("Name is required");

        if (errors.Any())
            throw new ValidationException(string.Join(", ", errors));
    }
}

// Issue: Entity validation mixed with business logic
public class Employee
{
    public void UpdateSalary(decimal newSalary)
    {
        // Validation mixed with business logic
        if (newSalary < 0)
            throw new ArgumentException("Salary must be positive");

        if (newSalary > CurrentSalary * 2)
            throw new BusinessException("Salary increase too high");

        Salary = newSalary;
    }
}
```

## 4. Incomplete Error Handling

```csharp
// Issue: Inconsistent error handling
public class PayrollController
{
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayroll(int departmentId)
    {
        try
        {
            // Process payroll...
            return Ok();
        }
        catch (Exception ex)
        {
            // Issue: Exposing internal error details
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayroll(int id)
    {
        try
        {
            var payroll = await _repository.GetByIdAsync(id);
            if (payroll == null)
                // Issue: Inconsistent 404 handling
                return NotFound($"Payroll {id} not found");

            return Ok(payroll);
        }
        catch (Exception)
        {
            // Issue: Swallowing exception details
            return StatusCode(500, "An error occurred");
        }
    }
}

// Issue: Missing error handling middleware
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        // Issue: Basic error handling only
        app.UseDeveloperExceptionPage();

        // Missing:
        // - Global exception handling
        // - Logging middleware
        // - Error response formatting
        // - Security headers
    }
}
```

## Impact

1. **Maintainability**
   - Scattered business logic
   - Inconsistent validation
   - Hard to test
   - Poor separation of concerns

2. **Reliability**
   - Inconsistent error handling
   - Missing validation
   - Hard dependencies
   - Poor error recovery

3. **Scalability**
   - Tight coupling
   - Hard to modify
   - Poor reusability
   - Testing difficulties

4. **Security**
   - Exposed error details
   - Missing validation
   - Poor error handling
   - Inconsistent responses

## Solution Reference
See the following implementations for solutions:
- [Service Layer Implementation](../service-layer-implementation.md)
- [Error Handling Implementation](error-handling-issues-fix.md)
- [Validation Implementation](../validation-implementation.md)
- [Dependency Injection Implementation](../dependency-injection-implementation.md)