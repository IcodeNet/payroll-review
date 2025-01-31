[⬅️ Back to Documentation](../../README.md)

# Department Implementations

## Implementation Reasoning

### Previous Issues
1. **Structure Problems**
   - Poor hierarchy management
   - Missing validation
   - Circular references
   - Weak relationships

2. **Employee Management**
   - Poor assignment tracking
   - Missing validations
   - Weak relationships
   - No capacity planning

3. **Organizational Changes**
   - No change tracking
   - Missing approvals
   - Poor audit trail
   - Weak validation

### Changes Made
1. **Hierarchy Management**
   - Tree structure
   - Cycle prevention
   - Strong validation
   - Clear boundaries

2. **Employee Handling**
   - Assignment tracking
   - Capacity management
   - Clear relationships
   - Transfer handling

3. **Change Management**
   - Change tracking
   - Approval workflow
   - Audit logging
   - Strong validation

### Benefits
1. **Organization**
   - Clear structure
   - Easy navigation
   - Better planning
   - Clear reporting

2. **Management**
   - Better oversight
   - Clear assignments
   - Easy transfers
   - Capacity planning

3. **Compliance**
   - Change tracking
   - Clear approvals
   - Audit support
   - Better reporting


```csharp
public class Department : AggregateRoot<int>
{
    private readonly List<Employee> _employees = new();
    private readonly List<Department> _subDepartments = new();

    private Department() { } // For EF Core

    private Department(
        DepartmentName name,
        Department? parentDepartment = null)
    {
        Name = name;
        ParentDepartment = parentDepartment;
        CreatedDate = DateTime.UtcNow;

        AddDomainEvent(new DepartmentCreatedEvent(this));
    }

    public static Result<Department> Create(
        DepartmentName name,
        Department? parentDepartment = null)
    {
        if (name == null)
            return Result.Failure<Department>("Department name is required");

        return Result.Success(new Department(name, parentDepartment));
    }

    public DepartmentName Name { get; private set; }
    public Department? ParentDepartment { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();
    public IReadOnlyCollection<Department> SubDepartments => _subDepartments.AsReadOnly();

    public Result UpdateName(DepartmentName newName)
    {
        if (newName == null)
            return Result.Failure("Department name cannot be null");

        var oldName = Name;
        Name = newName;

        AddDomainEvent(new DepartmentNameUpdatedEvent(Id, oldName, newName));

        return Result.Success();
    }

    public Result AddEmployee(Employee employee)
    {
        if (employee == null)
            return Result.Failure("Employee cannot be null");

        if (_employees.Any(e => e.Id == employee.Id))
            return Result.Failure("Employee already in department");

        _employees.Add(employee);
        AddDomainEvent(new EmployeeAddedToDepartmentEvent(Id, employee.Id));

        return Result.Success();
    }

    public Result RemoveEmployee(Employee employee)
    {
        if (employee == null)
            return Result.Failure("Employee cannot be null");

        if (!_employees.Any(e => e.Id == employee.Id))
            return Result.Failure("Employee not in department");

        _employees.Remove(employee);
        AddDomainEvent(new EmployeeRemovedFromDepartmentEvent(Id, employee.Id));

        return Result.Success();
    }

    public Result AddSubDepartment(Department department)
    {
        if (department == null)
            return Result.Failure("Department cannot be null");

        if (_subDepartments.Any(d => d.Id == department.Id))
            return Result.Failure("Sub-department already exists");

        if (WouldCreateCycle(department))
            return Result.Failure("Adding this department would create a cycle");

        _subDepartments.Add(department);
        department.ParentDepartment = this;

        AddDomainEvent(new SubDepartmentAddedEvent(Id, department.Id));

        return Result.Success();
    }

    private bool WouldCreateCycle(Department department)
    {
        var current = this;
        while (current.ParentDepartment != null)
        {
            if (current.ParentDepartment.Id == department.Id)
                return true;
            current = current.ParentDepartment;
        }
        return false;
    }
}

public class DepartmentName : ValueObject
{
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentName>("Department name cannot be empty");

        if (value.Length > 100)
            return Result.Failure<DepartmentName>("Department name cannot exceed 100 characters");

        return Result.Success(new DepartmentName(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

public class DepartmentCreatedEvent : DomainEvent
{
    public DepartmentCreatedEvent(Department department)
    {
        DepartmentId = department.Id;
        Name = department.Name.Value;
        ParentDepartmentId = department.ParentDepartment?.Id;
        OccurredOn = DateTime.UtcNow;
    }

    public int DepartmentId { get; }
    public string Name { get; }
    public int? ParentDepartmentId { get; }
    public DateTime OccurredOn { get; }
}

[Rest of domain events...]

## Key Improvements

1. **Hierarchy Management**
   - Proper parent-child relationships
   - Cycle prevention
   - Clear boundaries
   - Event tracking

2. **Employee Management**
   - Proper assignment
   - Validation rules
   - Event tracking
   - Clear relationships

3. **Domain Events**
   - Creation events
   - Update events
   - Employee events
   - Full audit trail

4. **Validation**
   - Name validation
   - Relationship rules
   - Cycle detection
   - Business rules