[⬅️ Back to Documentation](../../README.md)

# Service Layer Implementation

## Implementation Reasoning

### Previous Issues
1. **Architecture Problems**
   - Missing service layer
   - Direct repository access
   - Poor separation of concerns
   - Business logic in controllers

2. **Business Logic**
   - Scattered logic
   - Duplicate code
   - Poor validation
   - Inconsistent rules

3. **Integration Issues**
   - Tight coupling
   - Poor error handling
   - Missing transactions
   - Weak boundaries

### Changes Made
1. **Clean Architecture**
   - Service abstraction
   - CQRS pattern
   - Clear boundaries
   - Proper DI

2. **Business Rules**
   - Centralized logic
   - Strong validation
   - Consistent rules
   - Clear policies

3. **Integration**
   - Loose coupling
   - Error handling
   - Transaction management
   - Clear contracts

### Benefits
1. **Maintainability**
   - Clear structure
   - Easy testing
   - Better organization
   - Simple changes

2. **Reliability**
   - Consistent behavior
   - Better error handling
   - Clear boundaries
   - Easy debugging

3. **Scalability**
   - Easy extensions
   - Clear interfaces
   - Better monitoring
   - Simple integration

## Overview
This guide demonstrates how to implement a proper service layer between controllers and repositories, with dependency injection, validation, and error handling.

## 1. Service Layer Structure

```csharp
public interface IEmployeeService
{
    Task<Result<EmployeeDto>> GetEmployeeAsync(Guid id);
    Task<Result<IEnumerable<EmployeeDto>>> GetAllEmployeesAsync();
    Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeCommand command);
    Task<Result> UpdateEmployeeAsync(UpdateEmployeeCommand command);
    Task<Result> DeleteEmployeeAsync(Guid id);
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IValidator<CreateEmployeeCommand> _createValidator;
    private readonly IValidator<UpdateEmployeeCommand> _updateValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IValidator<CreateEmployeeCommand> createValidator,
        IValidator<UpdateEmployeeCommand> updateValidator,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeCommand command)
    {
        try
        {
            // Validate command
            var validationResult = await _createValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
                return Result.Failure<EmployeeDto>(validationResult.Errors);

            // Create employee
            var employee = await Employee.CreateAsync(command);
            if (employee.IsFailure)
                return Result.Failure<EmployeeDto>(employee.Error);

            // Save to repository
            await _employeeRepository.AddAsync(employee.Value);
            await _unitOfWork.SaveChangesAsync();

            // Map to DTO and return
            return Result.Success(employee.Value.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return Result.Failure<EmployeeDto>("An error occurred while creating the employee");
        }
    }
}
```

## 2. Dependency Injection Setup

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        // Register repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        // Register validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

## 3. Validation Strategy

```csharp
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.DepartmentId)
            .NotEmpty();

        RuleFor(x => x.Salary)
            .GreaterThan(0);
    }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .When(x => x.Name != null)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email != null);

        RuleFor(x => x.Salary)
            .GreaterThan(0)
            .When(x => x.Salary.HasValue);
    }
}
```

## 4. Error Handling Middleware

```csharp
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx =>
                (StatusCodes.Status400BadRequest,
                 new { Errors = validationEx.Errors }),

            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound,
                 new { Error = notFoundEx.Message }),

            UnauthorizedAccessException unauthorizedEx =>
                (StatusCodes.Status401Unauthorized,
                 new { Error = "Unauthorized access" }),

            _ => (StatusCodes.Status500InternalServerError,
                  new { Error = "An error occurred processing your request" })
        };

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(message);
    }
}

// Extension method to register the middleware
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
```

## 5. Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IEmployeeService employeeService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        CreateEmployeeCommand command)
    {
        var result = await _employeeService.CreateEmployeeAsync(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
```

## Key Improvements

1. **Service Layer**
   - Clear separation of concerns
   - Business logic encapsulation
   - Proper error handling
   - Logging integration

2. **Dependency Injection**
   - Proper service registration
   - Scoped lifetime management
   - Clear dependency chain
   - Easy testing support

3. **Validation**
   - Centralized validation rules
   - Reusable validators
   - Clear error messages
   - Consistent validation

4. **Error Handling**
   - Global error middleware
   - Proper status codes
   - Consistent error format
   - Detailed logging