namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Violates Smart Enum Pattern - enum lacks behavior
     * - Violates Open/Closed Principle - hard to extend
     * - Violates Domain Logic Encapsulation - business rules spread across system
     * - Violates Type Safety - no compile-time checking of valid operations
     * Should: Use Smart Enum pattern or Value Object
     * Should: Include contract type specific behavior
     * Should: Add validation rules
     * Should: Consider using class hierarchy instead
     */
    public enum ContractType
    {
        FullTime = 0,
        PartTime,
        Contractor
    }
}