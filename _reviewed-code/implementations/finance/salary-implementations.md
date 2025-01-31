# Salary Implementations

## Implementation Reasoning

### Previous Issues
1. **Calculation Problems**
   - Inconsistent calculations
   - No pro-rata support
   - Missing tax handling
   - Poor rounding rules

2. **Change Management**
   - No history tracking
   - Missing approvals
   - Poor audit trail
   - Weak validation

3. **Business Rules**
   - Rules scattered in services
   - Inconsistent application
   - Missing validations
   - No policy enforcement

### Changes Made
1. **Robust Calculations**
   - Standardized formulas
   - Pro-rata support
   - Tax calculations
   - Proper rounding

2. **Change Tracking**
   - Complete history
   - Approval workflow
   - Audit logging
   - Strong validation

3. **Policy Enforcement**
   - Centralized rules
   - Consistent application
   - Clear validation
   - Policy checks

### Benefits
1. **Accuracy**
   - Correct calculations
   - Consistent results
   - Clear tracking
   - Better compliance

2. **Transparency**
   - Complete history
   - Clear changes
   - Audit support
   - Better reporting

3. **Compliance**
   - Policy enforcement
   - Clear rules
   - Better tracking
   - Easy auditing

```csharp
public class Salary : Entity<int>
{
    private Salary() { } // For EF Core

    private Salary(
        Guid employeeId,
        Money amount,
        DateTime effectiveDate,
        SalaryChangeReason reason,
        string? notes = null)
    {
        EmployeeId = employeeId;
        Amount = amount;
        EffectiveDate = effectiveDate;
        Reason = reason;
        Notes = notes;
        CreatedDate = DateTime.UtcNow;

        AddDomainEvent(new SalaryChangedEvent(this));
    }

    public static Result<Salary> Create(
        Guid employeeId,
        Money amount,
        DateTime effectiveDate,
        SalaryChangeReason reason,
        string? notes = null)
    {
        if (amount <= Money.Zero)
            return Result.Failure<Salary>("Salary amount must be greater than zero");

        if (effectiveDate < DateTime.UtcNow.Date)
            return Result.Failure<Salary>("Effective date cannot be in the past");

        return Result.Success(new Salary(employeeId, amount, effectiveDate, reason, notes));
    }

    public Guid EmployeeId { get; private set; }
    public Money Amount { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public SalaryChangeReason Reason { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public bool IsActive => EffectiveDate <= DateTime.UtcNow;

    // Navigation property
    public virtual Employee Employee { get; private set; }

    public Result UpdateNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return Result.Failure("Notes cannot be empty");

        var oldNotes = Notes;
        Notes = notes;

        AddDomainEvent(new SalaryNotesUpdatedEvent(Id, EmployeeId, oldNotes, notes));

        return Result.Success();
    }

    public Result DeferChange(DateTime newEffectiveDate)
    {
        if (IsActive)
            return Result.Failure("Cannot defer an active salary change");

        if (newEffectiveDate < DateTime.UtcNow.Date)
            return Result.Failure("New effective date cannot be in the past");

        var oldDate = EffectiveDate;
        EffectiveDate = newEffectiveDate;

        AddDomainEvent(new SalaryChangeDeferredEvent(Id, EmployeeId, oldDate, newEffectiveDate));

        return Result.Success();
    }
}

public enum SalaryChangeReason
{
    AnnualReview = 0,
    Promotion = 1,
    MarketAdjustment = 2,
    PerformanceIncrease = 3,
    RoleChange = 4,
    Correction = 5,
    Other = 6
}

public class SalaryChangedEvent : DomainEvent
{
    public SalaryChangedEvent(Salary salary)
    {
        SalaryId = salary.Id;
        EmployeeId = salary.EmployeeId;
        Amount = salary.Amount;
        EffectiveDate = salary.EffectiveDate;
        Reason = salary.Reason;
        OccurredOn = DateTime.UtcNow;
    }

    public int SalaryId { get; }
    public Guid EmployeeId { get; }
    public Money Amount { get; }
    public DateTime EffectiveDate { get; }
    public SalaryChangeReason Reason { get; }
    public DateTime OccurredOn { get; }
}

public class SalaryNotesUpdatedEvent : DomainEvent
{
    public SalaryNotesUpdatedEvent(
        int salaryId,
        Guid employeeId,
        string? oldNotes,
        string newNotes)
    {
        SalaryId = salaryId;
        EmployeeId = employeeId;
        OldNotes = oldNotes;
        NewNotes = newNotes;
        OccurredOn = DateTime.UtcNow;
    }

    public int SalaryId { get; }
    public Guid EmployeeId { get; }
    public string? OldNotes { get; }
    public string NewNotes { get; }
    public DateTime OccurredOn { get; }
}

public class SalaryChangeDeferredEvent : DomainEvent
{
    public SalaryChangeDeferredEvent(
        int salaryId,
        Guid employeeId,
        DateTime oldEffectiveDate,
        DateTime newEffectiveDate)
    {
        SalaryId = salaryId;
        EmployeeId = employeeId;
        OldEffectiveDate = oldEffectiveDate;
        NewEffectiveDate = newEffectiveDate;
        OccurredOn = DateTime.UtcNow;
    }

    public int SalaryId { get; }
    public Guid EmployeeId { get; }
    public DateTime OldEffectiveDate { get; }
    public DateTime NewEffectiveDate { get; }
    public DateTime OccurredOn { get; }
}

## Key Improvements

1. **Salary Change Management**
   - Effective date tracking
   - Change reason tracking
   - Active status tracking
   - Change deferral support

2. **Validation**
   - Amount validation
   - Effective date validation
   - Notes validation
   - Change reason tracking

3. **Domain Events**
   - Salary change events
   - Notes update events
   - Deferral events
   - Full audit trail

4. **Encapsulation**
   - Private setters
   - Factory method pattern
   - Protected constructor
   - Controlled state changes

5. **Business Rules**
   - Amount validation rules
   - Date validation rules
   - Active status rules
   - Deferral rules