namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Violates Liskov Substitution Principle - overrides without extending behavior
     * - Violates Open/Closed Principle - hard-coded values
     * - Violates Domain Logic Encapsulation - business rules not explicit
     * - Violates Factory Pattern - complex creation not encapsulated
     */
    public class FullTimeEmployee : Employee
    {
        /*
         * Issue: Constructor
         * Violates: Fail-Fast Principle - no validation
         * Violates: Factory Pattern - complex creation not encapsulated
         * Should: Validate state in constructor
         * Should: Use factory method
         */
        public FullTimeEmployee()
        {
            ContractType = ContractType.FullTime;
        }

        /*
         * Issue: Holiday allowance
         * Violates: Configuration Principle - hard-coded value
         * Violates: Business Rules Encapsulation - implicit rule
         * Should: Use configuration
         * Should: Make business rules explicit
         */
        public override int AnnualHolidayAllowance => 25;
    }
}
