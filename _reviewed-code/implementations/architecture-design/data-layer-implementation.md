[⬅️ Back to Documentation](../../README.md)

# Data Layer Implementation

## Implementation Reasoning

### Previous Issues
1. **Poor Data Access Patterns**
   - Direct entity manipulation
   - No repository pattern
   - Missing unit of work
   - Inefficient queries

2. **Missing Domain Events**
   - No event publishing
   - Poor state tracking
   - Missing audit trail
   - Inconsistent updates

3. **Configuration Issues**
   - Hard-coded mappings
   - Missing indexes
   - Poor relationship definitions
   - No value object support

### Changes Made
1. **Implemented Repository Pattern**
   - Generic repository base
   - Specification pattern
   - Unit of work
   - Async operations

2. **Added Domain Events**
   - Event dispatching
   - State tracking
   - Audit support
   - Consistent updates

3. **Improved Configuration**
   - Fluent configurations
   - Proper indexing
   - Clear relationships
   - Value object support

### Benefits
1. **Performance**
   - Efficient queries
   - Proper caching
   - Better indexing
   - Reduced database load

2. **Maintainability**
   - Clean architecture
   - Easy to test
   - Clear patterns
   - Better organization

3. **Data Integrity**
   - Proper validation
   - Consistent updates
   - Better tracking
   - Clear audit trail

## 1. DbContext Implementation

```csharp
public class PayrollContext : DbContext
{
    private readonly IMediator _mediator;

    public PayrollContext(
        DbContextOptions<PayrollContext> options,
        IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<PaymentHistory> PaymentHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dispatchDomainEvents();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task _dispatchDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity);

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ToList().ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent);
    }
}
```

## 2. Repository Implementation

```csharp
public abstract class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly PayrollContext Context;
    protected readonly DbSet<T> DbSet;

    protected Repository(PayrollContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(DbSet.AsQueryable(), spec);
    }
}
```

## 3. Entity Configurations

```csharp
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasConversion(
                name => name.ToString(),
                value => EmployeeName.Create(value).Value)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Salary)
            .HasConversion(
                money => money.Amount,
                value => Money.Create(value).Value)
            .IsRequired();

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => new { e.Name, e.DepartmentId });
    }
}
```

## 4. Unit of Work

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly PayrollContext _context;
    private readonly IDictionary<Type, object> _repositories;

    public UnitOfWork(PayrollContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<T> Repository<T>() where T : AggregateRoot
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(type);
            _repositories[type] = Activator.CreateInstance(repositoryType, _context);
        }

        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
```

## Key Improvements

1. **Data Access**
   - Generic repository
   - Specification pattern
   - Unit of work
   - Domain event dispatch

2. **Entity Configuration**
   - Proper mapping
   - Value object conversion
   - Index configuration
   - Relationship mapping

3. **Performance**
   - Efficient querying
   - Proper indexing
   - Lazy loading
   - Change tracking

4. **Maintainability**
   - Clean architecture
   - Separation of concerns
   - Reusable components
   - Clear patterns