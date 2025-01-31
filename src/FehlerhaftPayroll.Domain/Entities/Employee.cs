using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Anemic domain model (only properties, no business rules)
     * - Public setters exposing internal state (violates encapsulation)
     * - Missing validation and business rules (e.g., salary ranges, name format)
     * - Poor encapsulation (collections and properties directly exposed)
     * - Missing domain events (for important state changes)
     * - No aggregate root implementation (missing invariant checks)
     * - Missing value objects (Name, Salary should be value objects with validation)
     * - No audit trail for changes
     * - Missing business rule enforcement (e.g., holiday calculations)
     */
    public class Employee : IAggregate<Guid>
    {
        public const int Default_Holiday_Allowance_Days = 25;

        /*
         * Issue: Public setters allow direct state modification
         * Should: Use private setters and provide methods for state changes
         */
        public Guid Id { get; set; }

        /*
         * Issue: Enum used where value object would be more appropriate
         * Should: Create ContractType value object with validation and business rules
         */
        public ContractType ContractType { get; set; }

        /*
         * Issue: No validation on holiday allowance changes
         * Should: Validate against business rules (min/max days, pro-rata calculations)
         */
        public virtual int AnnualHolidayAllowance { get; set; } = Default_Holiday_Allowance_Days;

        /*
         * Issue: Name should be a value object
         * Should: Create Name value object with proper validation (length, format, culture)
         */
        public string Name { get; set; }

        /*
         * Issue: Bank details exposed without protection
         * Should: Encapsulate and provide methods for controlled updates
         * Should: Include validation for banking details changes
         */
        public BankDetails BankDetails { get; set; }

        /*
         * Issue: No validation on salary changes
         */
        public decimal AnnualPay { get; set; }

        /*
         * Issue: Missing validation in update method
         */
        public virtual void UpdateEmployee(string name, int? holidayAllowance)
        {
            Name = name ?? Name;
            AnnualHolidayAllowance = holidayAllowance.GetValueOrDefault(AnnualHolidayAllowance);
        }
    }
}
