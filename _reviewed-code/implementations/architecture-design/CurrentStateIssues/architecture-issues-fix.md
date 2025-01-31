[⬅️ Back to Documentation](../../../README.md)

# Architecture Issues - Fixed Implementation

## 1. Service Layer - Fixed

```csharp
// Command/Query pattern for clear separation
public record CreateEmployeeCommand(string Name, decimal Salary, int DepartmentId);
public record UpdateEmployeeCommand(Guid Id, string Name, decimal Salary);
public record ProcessPayrollCommand(int DepartmentId, DateTime PayPeriodStart, DateTime PayPeriodEnd);

// Service layer with proper business logic encapsulation
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IValidator<CreateEmployeeCommand> _createValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository repository,
        IValidator<CreateEmployeeCommand> createValidator,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<EmployeeDto>> CreateAsync(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate command
            var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<EmployeeDto>(validationResult.Errors);

            // Create employee through domain entity
            var employeeResult = await Employee.CreateAsync(
                new EmployeeName(command.Name),
                Money.Create(command.Salary),
                command.DepartmentId);

            if (employeeResult.IsFailure)
                return Result.Failure<EmployeeDto>(employeeResult.Error);

            // Save and return
            await _repository.AddAsync(employeeResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(_mapper.Map<EmployeeDto>(employeeResult.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return Result.Failure<EmployeeDto>("An error occurred while creating the employee");
        }
    }
}

// Clean controller using service layer
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(new ErrorResponse(result.Error));
    }
}
```

## 2. Dependency Injection - Fixed

```csharp
// Proper DI setup
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IEmailService, EmailService>();

        // Register repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();

        // Register infrastructure
        services.AddDbContext<PayrollContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }
}

// Proper service implementation with injected dependencies
public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<PayrollService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public PayrollService(
        IPayrollRepository repository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<PayrollService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ProcessPaymentAsync(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await Payment.CreateAsync(command);
            if (payment.IsFailure)
                return Result.Failure(payment.Error);

            await _repository.AddAsync(payment.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _emailService.SendPaymentNotificationAsync(payment.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return Result.Failure("An error occurred while processing the payment");
        }
    }
}
```

## 3. Validation Strategy - Fixed

```csharp
// Command validators
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Salary)
            .GreaterThan(0)
            .LessThan(1000000);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0);
    }
}

// Domain validation
public class Employee : AggregateRoot<Guid>
{
    private Employee() { } // For EF Core

    private Employee(
        Guid id,
        EmployeeName name,
        Money salary,
        Department department)
    {
        Id = Guard.Against.Default(id, nameof(id));
        Name = Guard.Against.Null(name, nameof(name));
        Salary = Guard.Against.Null(salary, nameof(salary));
        Department = Guard.Against.Null(department, nameof(department));
    }

    public static async Task<Result<Employee>> CreateAsync(
        EmployeeName name,
        Money salary,
        Department department)
    {
        try
        {
            var employee = new Employee(Guid.NewGuid(), name, salary, department);
            employee.AddDomainEvent(new EmployeeCreatedEvent(employee));

            return Result.Success(employee);
        }
        catch (Exception ex)
        {
            return Result.Failure<Employee>(ex.Message);
        }
    }

    public Result UpdateSalary(Money newSalary)
    {
        if (newSalary <= Money.Zero)
            return Result.Failure("Salary must be greater than zero");

        if (newSalary > Salary * 2)
            return Result.Failure("Salary increase cannot exceed 100%");

        var oldSalary = Salary;
        Salary = newSalary;

        AddDomainEvent(new EmployeeSalaryUpdatedEvent(Id, oldSalary, newSalary));

        return Result.Success();
    }
}
```

## 4. Error Handling - Fixed

```csharp
// Global error handling middleware
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx =>
                (StatusCodes.Status400BadRequest,
                 new ErrorResponse("Validation Error", validationEx.Errors)),

            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound,
                 new ErrorResponse(notFoundEx.Message)),

            DomainException domainEx =>
                (StatusCodes.Status400BadRequest,
                 new ErrorResponse(domainEx.Message)),

            _ => (StatusCodes.Status500InternalServerError,
                 new ErrorResponse("An unexpected error occurred"))
        };

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(message);
    }
}

// Startup configuration
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}
```

## Key Improvements

1. **Service Layer**
   - Clean separation of concerns
   - Business logic encapsulation
   - Command/Query pattern
   - Proper validation

2. **Dependency Injection**
   - Proper DI configuration
   - Interface-based design
   - Scoped services
   - Testable components

3. **Validation**
   - Consistent strategy
   - Command validation
   - Domain validation
   - Clear error messages

4. **Error Handling**
   - Global error handling
   - Consistent responses
   - Proper logging
   - Security considerations