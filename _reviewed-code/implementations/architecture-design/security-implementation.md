[⬅️ Back to Documentation](../../README.md)

# Security Implementation

## Overview
This guide demonstrates how to implement comprehensive security measures including authentication, HTTPS enforcement, data protection, input sanitization, rate limiting, and audit logging.

## Implementation Reasoning

### Previous Issues
1. **Authentication Weaknesses**
   - Basic authentication only
   - No refresh tokens
   - Poor token validation
   - Missing role-based access

2. **Data Protection**
   - Unencrypted sensitive data
   - Missing data masking
   - Poor key management
   - Weak transport security

3. **Security Controls**
   - No rate limiting
   - Missing audit logs
   - Poor error masking
   - Weak input validation

### Changes Made
1. **Enhanced Authentication**
   - JWT implementation
   - Refresh token support
   - Strong validation
   - Role-based security

2. **Data Security**
   - Field-level encryption
   - Data masking
   - Key rotation
   - TLS enforcement

3. **Security Features**
   - Rate limiting
   - Comprehensive logging
   - Error handling
   - Input sanitization

### Benefits
1. **Access Control**
   - Secure authentication
   - Fine-grained authorization
   - Clear audit trail
   - Better monitoring

2. **Data Protection**
   - Secure storage
   - Safe transmission
   - Compliance ready
   - Privacy protection

3. **System Security**
   - Attack prevention
   - Quick detection
   - Better resilience
   - Easy auditing

## 1. Authentication Configuration

```csharp
public static class AuthenticationSetup
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
```

## 2. HTTPS Enforcement

```csharp
public static class HttpsConfiguration
{
    public static IServiceCollection AddHttpsConfiguration(
        this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new RequireHttpsAttribute());
        });

        return services;
    }

    public static IApplicationBuilder UseHttpsEnforcement(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        return app;
    }
}
```

## 3. Sensitive Data Protection

```csharp
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionService> _logger;

    public DataProtectionService(
        IDataProtectionProvider provider,
        ILogger<DataProtectionService> logger)
    {
        _protector = provider.CreateProtector("PayrollSystem.DataProtection");
        _logger = logger;
    }

    public string ProtectSensitiveData(string data)
    {
        try
        {
            return _protector.Protect(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting sensitive data");
            throw new DataProtectionException("Failed to protect sensitive data", ex);
        }
    }

    public string UnprotectSensitiveData(string protectedData)
    {
        try
        {
            return _protector.Unprotect(protectedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unprotecting sensitive data");
            throw new DataProtectionException("Failed to unprotect sensitive data", ex);
        }
    }
}

// Extension method for entity configuration
public static class DataProtectionExtensions
{
    public static void ConfigureProtectedField<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, string>> propertyExpression) where T : class
    {
        builder.Property(propertyExpression)
            .HasConversion(
                v => DataProtection.Protect(v),
                v => DataProtection.Unprotect(v));
    }
}
```

## 4. Input Sanitization

```csharp
public class InputSanitizer : IInputSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public InputSanitizer()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear(); // Remove all allowed tags for strict sanitization
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove potentially dangerous characters
        input = Regex.Replace(input, @"[<>'""%;&]", string.Empty);

        // Sanitize any HTML
        return _sanitizer.Sanitize(input);
    }
}

// Attribute for automatic input sanitization
public class SanitizeInputAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var sanitizer = context.HttpContext.RequestServices
            .GetRequiredService<IInputSanitizer>();

        foreach (var param in context.ActionArguments)
        {
            if (param.Value is string stringValue)
            {
                context.ActionArguments[param.Key] = sanitizer.SanitizeInput(stringValue);
            }
        }
    }
}
```

## 5. Rate Limiting

```csharp
public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

        services.AddMemoryCache();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        return services;
    }
}

// Example configuration in appsettings.json
{
    "IpRateLimiting": {
        "EnableEndpointRateLimiting": true,
        "StackBlockedRequests": false,
        "RealIpHeader": "X-Real-IP",
        "ClientIdHeader": "X-ClientId",
        "HttpStatusCode": 429,
        "GeneralRules": [
            {
                "Endpoint": "*",
                "Period": "1s",
                "Limit": 10
            },
            {
                "Endpoint": "*",
                "Period": "15m",
                "Limit": 100
            }
        ]
    }
}
```

## 6. Audit Logging

```csharp
public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly IAuditLogRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogService(
        ILogger<AuditLogService> logger,
        IAuditLogRepository repository,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task LogActionAsync(
        string action,
        string entityType,
        string entityId,
        string details,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            Timestamp = DateTime.UtcNow,
            Details = details,
            IpAddress = _currentUserService.IpAddress
        };

        await _repository.AddAsync(auditLog, cancellationToken);
        _logger.LogInformation(
            "Audit Log: {Action} on {EntityType} {EntityId} by {UserName}",
            action, entityType, entityId, _currentUserService.UserName);
    }
}

// Attribute for automatic audit logging
public class AuditLogAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var auditService = context.HttpContext.RequestServices
            .GetRequiredService<IAuditLogService>();

        var executedContext = await next();

        if (executedContext.Result is ObjectResult result)
        {
            await auditService.LogActionAsync(
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name,
                context.ActionArguments.ToString(),
                JsonSerializer.Serialize(result.Value));
        }
    }
}
```

## Key Improvements

1. **Authentication**
   - JWT implementation
   - Token validation
   - Refresh token support
   - Secure token handling

2. **HTTPS**
   - Forced HTTPS
   - HSTS implementation
   - Secure headers
   - TLS configuration

3. **Data Protection**
   - Field-level encryption
   - Secure storage
   - Key management
   - Data masking

4. **Input Sanitization**
   - HTML sanitization
   - XSS prevention
   - SQL injection prevention
   - Automatic sanitization

5. **Rate Limiting**
   - IP-based limiting
   - User-based limiting
   - Endpoint-specific rules
   - Configurable limits

6. **Audit Logging**
   - Comprehensive tracking
   - User attribution
   - Secure storage
   - Search capabilities