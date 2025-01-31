[⬅️ Back to Documentation](../../README.md)

# Performance Implementation

## Implementation Reasoning

### Previous Issues
1. **Caching Problems**
   - No caching strategy
   - Cache invalidation issues
   - Memory leaks
   - Inconsistent data

2. **Query Performance**
   - N+1 queries
   - Missing indexes
   - Poor eager loading
   - Inefficient joins

3. **Resource Usage**
   - High memory usage
   - CPU bottlenecks
   - Connection pool exhaustion
   - Thread starvation

### Changes Made
1. **Caching Strategy**
   - Distributed caching
   - Proper invalidation
   - Memory management
   - Cache policies

2. **Query Optimization**
   - Efficient queries
   - Strategic indexes
   - Smart loading
   - Query optimization

3. **Resource Management**
   - Connection pooling
   - Thread management
   - Memory optimization
   - Resource monitoring

### Benefits
1. **Response Time**
   - Faster responses
   - Better scalability
   - Reduced latency
   - Improved UX

2. **Resource Efficiency**
   - Lower costs
   - Better utilization
   - Reduced load
   - Higher throughput

3. **Scalability**
   - Better performance
   - Easy scaling
   - Clear monitoring
   - Resource optimization

## Overview
This guide demonstrates how to implement performance optimizations including caching, query optimization, pagination, and efficient data loading strategies.

## 1. Caching Strategy - Fixed

```csharp
public class CachedEmployeeService
{
    private readonly IDistributedCache _cache;
    private readonly PayrollContext _context;
    private readonly ILogger<CachedEmployeeService> _logger;

    public async Task<Employee> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"employee:{id}";

        // Try get from cache
        var employee = await _cache.GetAsync<Employee>(cacheKey, cancellationToken);
        if (employee != null)
            return employee;

        // Get from database and cache
        employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee != null)
        {
            await _cache.SetAsync(cacheKey, employee,
                TimeSpan.FromMinutes(30), cancellationToken);
        }

        return employee;
    }

    public async Task<List<Employee>> GetByDepartmentAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"department:{departmentId}:employees";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async () => await _context.Employees
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync(cancellationToken),
            TimeSpan.FromMinutes(15),
            cancellationToken);
    }
}
```

## 2. Query Optimization - Fixed

```csharp
public class DepartmentController
{
    private readonly PayrollContext _context;

    public async Task<IActionResult> GetDepartmentSummaries(
        CancellationToken cancellationToken)
    {
        // Single optimized query
        var summaries = await _context.Departments
            .Select(d => new DepartmentSummaryDto
            {
                Id = d.Id,
                Name = d.Name,
                EmployeeCount = d.Employees.Count,
                TotalSalary = d.Employees.Sum(e => e.Salary)
            })
            .ToListAsync(cancellationToken);

        return Ok(summaries);
    }
}

public class EmployeeRepository
{
    public async Task<List<EmployeeSummaryDto>> GetEmployeeSummariesAsync(
        Expression<Func<Employee, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        // Optimized projection query
        return await _context.Employees
            .AsNoTracking()
            .Where(filter)
            .Select(e => new EmployeeSummaryDto
            {
                Id = e.Id,
                Name = e.Name,
                Department = e.Department.Name,
                Status = e.Status
            })
            .ToListAsync(cancellationToken);
    }
}
```

## 3. Pagination Implementation - Fixed

```csharp
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}

public class PaymentHistoryController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<PaymentDto>>> GetAll(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentHistory
            .AsNoTracking()
            .Include(p => p.Employee)
            .OrderByDescending(p => p.ProcessedDate);

        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                EmployeeName = p.Employee.Name,
                ProcessedDate = p.ProcessedDate
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<PaymentDto>(items, count, pageIndex, pageSize);
    }
}
```

## 4. Efficient Loading - Fixed

```csharp
public class EmployeeRepository
{
    public async Task<EmployeeDetailsDto> GetDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Selective loading based on DTO needs
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDetailsDto
            {
                Id = e.Id,
                Name = e.Name,
                Department = e.Department.Name,
                RecentPayments = e.PaymentHistory
                    .OrderByDescending(p => p.ProcessedDate)
                    .Take(5)
                    .Select(p => new PaymentDto
                    {
                        Amount = p.Amount,
                        Date = p.ProcessedDate
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EmployeeSummaryDto>> GetSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        // Efficient projection for summaries
        return await _context.Employees
            .AsNoTracking()
            .Select(e => new EmployeeSummaryDto
            {
                Id = e.Id,
                Name = e.Name,
                Department = e.Department.Name
            })
            .ToListAsync(cancellationToken);
    }
}
```

## 5. Database Indexing - Fixed

```csharp
public class PayrollContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Frequently filtered fields
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DepartmentId);

            // Composite index for name searches
            entity.HasIndex(e => new { e.LastName, e.FirstName });

            // Index for common queries
            entity.HasIndex(e => new { e.DepartmentId, e.Status });
        });

        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            // Primary key
            entity.HasKey(p => p.Id);

            // Date range queries
            entity.HasIndex(p => p.ProcessedDate);

            // Common lookup patterns
            entity.HasIndex(p => new { p.EmployeeId, p.Status });
            entity.HasIndex(p => new { p.ProcessedDate, p.Status });

            // Foreign key relationship
            entity.HasIndex(p => p.EmployeeId);
        });
    }
}

// Optimized queries using indexes
public class PayrollRepository
{
    public async Task<List<Payment>> GetPaymentsByDateRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.ProcessedDate >= start &&
                       p.ProcessedDate <= end)
            .OrderBy(p => p.ProcessedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Employee>> SearchEmployeesAsync(
        string lastName,
        string firstName,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsNoTracking();

        if (!string.IsNullOrEmpty(lastName))
            query = query.Where(e => e.LastName.StartsWith(lastName));

        if (!string.IsNullOrEmpty(firstName))
            query = query.Where(e => e.FirstName.StartsWith(firstName));

        return await query.ToListAsync(cancellationToken);
    }
}
```

## Key Improvements

1. **Caching Strategy**
   - Distributed caching
   - Cache invalidation
   - Configurable timeouts
   - Memory optimization

2. **Query Optimization**
   - Single query solutions
   - Efficient projections
   - No-tracking queries
   - Proper includes

3. **Pagination**
   - Efficient data retrieval
   - Consistent API
   - Memory efficiency
   - Performance metrics

4. **Data Loading**
   - Selective loading
   - DTO projections
   - Minimal data transfer
   - Efficient queries

5. **Database Indexes**
   - Strategic indexing
   - Composite indexes
   - Query optimization
   - Performance monitoring