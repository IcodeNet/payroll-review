namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Violates Liskov Substitution Principle - changes behavior without extension
     * - Violates Open/Closed Principle - hard-coded calculations
     * - Violates Domain Logic Encapsulation - business rules not explicit
     * - Violates Complete Abstraction Principle - missing working hours concept
     */
    public class PartTimeEmployee : Employee
    {
        /*
         * Issue: Constructor
         * Violates: Fail-Fast Principle - no validation
         * Violates: Factory Pattern - complex creation not encapsulated
         * Violates: Domain Completeness - missing required state
         * Should: Validate initial state
         * Should: Use factory method
         * Should: Include working pattern
         */
        public PartTimeEmployee()
        {
            ContractType = ContractType.PartTime;
        }

        /*
         * Issue: Holiday calculation
         * Violates: Business Rules Encapsulation - implicit calculation
         * Violates: Domain Completeness - missing working hours factor
         * Violates: Configuration Principle - hard-coded value
         * Should: Calculate based on hours
         * Should: Make rules explicit
         * Should: Use configuration
         */
        public override int AnnualHolidayAllowance => 12;
    }
}
