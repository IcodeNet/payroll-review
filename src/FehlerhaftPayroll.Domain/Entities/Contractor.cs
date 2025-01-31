using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Poor contract management (Violates Domain Invariants Principle)
     * - Missing contract period validation (Violates Fail-Fast Principle)
     * - No renewal tracking (Violates Audit Trail Principle)
     * - Weak termination rules (Violates Business Rules Encapsulation)
     * - Missing rate history (Violates Temporal Modeling Principle)
     * - Poor validation of contract terms (Violates Domain Integrity Principle)
     */
    public class Contractor : Employee
    {
        /*
         * Issue: Constructor implementation
         * Violates: Fail-Fast Principle - no validation
         * Violates: Factory Pattern - complex object creation not encapsulated
         * Should: Validate contract terms in constructor
         * Should: Use factory method for creation
         */
        public Contractor()
        {
            ContractType = ContractType.Contractor;
        }

        #region "properties"
        /*
         * Issue: Contract expiry
         * Violates: Encapsulation Principle - public setter allows invalid dates
         * Violates: Temporal Modeling - no history tracking
         * Violates: Domain Events - changes not tracked
         * Should: Add validation for dates
         * Should: Track contract history
         * Should: Raise events on changes
         */
        public DateTime ContractExpires { get; set; }
        #endregion

        #region "compound properties"
        /*
         * Issue: Holiday allowance implementation
         * Violates: Tell Don't Ask Principle - throws instead of handling
         * Violates: Domain Logic Encapsulation - business rule through exception
         * Should: Return proper domain result
         * Should: Handle special cases properly
         */
        public override int AnnualHolidayAllowance =>
            throw new ArgumentOutOfRangeException($"No holiday allowance for contractors");
        #endregion
    }
}