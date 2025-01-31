namespace FehlerhaftPayroll.Banking
{
    /*
     * Architecture Issues:
     * - Violates Clean Code Principles - unclear logic and comments
     * - Violates Single Responsibility Principle - mixing status and formatting
     * - Violates Open/Closed Principle - hard to extend status types
     * - Violates Domain Logic Encapsulation - business rule in extension
     */
    public static class FakePaymentExtension
    {
        /*
         * Issue: Status implementation
         * Violates: Domain-Driven Design - magic strings
         * Violates: Clean Code - unclear logic
         * Violates: Error Handling Pattern - poor status representation
         * Should: Use proper enum or status object
         * Should: Make logic clear and documented
         * Should: Follow domain language
         */
        public static string ToStatus(this FakePayment payment)
        {
            // I wasn't sure what to put here
            // It seemed that the status could be good
            // but that's only if the payment is null
            // otherwise we'll return bad, and use emphasis!
            return (payment != null) ? "GOOD!" : "BAD!";
        }
    }
}