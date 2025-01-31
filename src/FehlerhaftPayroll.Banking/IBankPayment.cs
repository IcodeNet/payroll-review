using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FehlerhaftPayroll.Domain.Entities;

namespace FehlerhaftPayroll.Banking
{
    /*
     * Architecture Issues:
     * - Interface tries to do too many things (Violates Interface Segregation Principle)
     * - Should be split into separate interfaces (Violates Single Responsibility Principle)
     * - Inconsistent async/sync method usage (Violates Consistency Principle)
     * - Poor separation of concerns (Violates Separation of Concerns)
     * - Missing proper documentation (Violates Documentation Principle)
     * - No error handling strategy (Violates Error Handling Pattern)
     * - Missing payment validation (Violates Validation Pattern)
     */
    public interface IBankPayment
    {
        /*
         * Issue: Payment method
         * Violates: Command Query Separation - mixing concerns
         * Violates: Interface Segregation - too many parameters
         * Violates: Parameter Object Pattern - primitive obsession
         * Should: Use payment request object
         * Should: Split into smaller interfaces
         */
        Task<BankResult> MakePaymentAsync(string reference, string sortCode, string accountNumber, double amount,
            CancellationToken cancellationToken = default);

        /*
         * Issue: Status method
         * Violates: Return Type Pattern - using string instead of enum/object
         * Violates: Domain-Driven Design - poor domain representation
         * Should: Use proper status type
         * Should: Follow domain language
         */
        Task<string> GetPaymentStatusAsync(string reference, CancellationToken cancellationToken = default);

        /*
         * Issue: Receipt method
         * Violates: Interface Segregation - UI concern in payment interface
         * Violates: Single Responsibility - mixing payment and printing
         * Should: Move to separate interface
         * Should: Use proper abstraction
         */
        Task PrintReceiptAsync(string reference, string sortCode, string accountNumber, double amount,
            CancellationToken cancellationToken = default);

        /*
         * Issue: Synchronous method in async interface
         * Issue: Missing validation for payment periods
         */
        BankResult PayEmployee(Employee employee, DateTime periodStart, DateTime periodEnd, string reference);
    }
}
