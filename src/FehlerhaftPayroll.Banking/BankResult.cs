namespace FehlerhaftPayroll.Banking
{
    /*
     * Domain Issues:
     * - Violates Result Pattern - missing proper result structure
     * - Violates Tell Don't Ask Principle - exposes state directly
     * - Violates Encapsulation - public setters
     * - Violates Domain Events - no tracking of state changes
     * Should: Use proper Result pattern with error details
     * Should: Add validation and error information
     * Should: Make properties immutable
     * Should: Consider using discriminated union pattern
     */
    public class BankResult
    {
        /*
         * Issue: Result implementation
         * Violates: Immutability Principle - mutable state
         * Violates: Fail-Fast Principle - defaults to success
         * Should: Use constructor to set state
         * Should: Make properties read-only
         */
        public bool IsSuccess { get; set; } = true;

        /*
         * Issue: Message handling
         * Violates: Null Object Pattern - no handling of null
         * Violates: Error Handling Pattern - mixing success/error messages
         * Should: Separate success/error messages
         * Should: Add proper error details
         */
        public string ResponseMessage { get; set; }
    }
}