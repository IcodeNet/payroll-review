using System;
using System.Threading;
using System.Threading.Tasks;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FehlerhaftPayroll.Banking
{
    /*
     * Architecture Issues:
     * - Poor error handling (Violates Robustness Principle)
     * - Missing retry policies (Violates Resilience Pattern)
     * - No circuit breaker pattern (Violates Stability Pattern)
     * - Missing proper logging strategy (Violates Observability Principle)
     * - No payment validation (Violates Validation Pattern)
     * - Missing transaction handling (Violates ACID Principles)
     * - Poor exception handling (Violates Error Handling Pattern)
     */
    public class FakePayment : IBankPayment
    {
        /*
         * Issue: Logger implementation
         * Violates: Dependency Inversion Principle - concrete creation
         * Violates: Dependency Injection - manual creation
         * Should: Inject logger through constructor
         * Should: Use proper logging abstraction
         */
        private readonly ILogger<FakePayment> _logger;

        public FakePayment()
        {
            /*
             * Issue: Direct logger instantiation instead of DI
             */
            _logger = new Logger<FakePayment>(new NullLoggerFactory());
        }

        /*
         * Issue: Payment implementation
         * Violates: Command Query Separation - mixing calculation and payment
         * Violates: Single Responsibility Principle - multiple operations
         * Violates: Async/Await Best Practices - blocking on async
         * Should: Separate calculation logic
         * Should: Use proper async/await
         * Should: Add validation
         */
        public BankResult PayEmployee(Employee employee, DateTime periodStart, DateTime periodEnd, string paymentReference)
        {
            /*
             * Issue: No error handling
             * Issue: Unsafe calculation without validation
             */
            var paymentPeriodDays = periodEnd - periodStart;
            var amountToPay = (double)(employee.AnnualPay * paymentPeriodDays.Days);

            /*
             * Issue: Poor logging - missing structured logging
             */
            _logger.LogInformation($"Paying {employee.Name}, {amountToPay:C} for the period {periodStart} to {periodEnd}");

            /*
             * Issue: Sync over async call
             */
            return MakePaymentAsync(
                paymentReference,
                employee.BankDetails.SortCode,
                employee.BankDetails.AccountNumber,
                amountToPay,
                CancellationToken.None).Result;
        }

        public Task<BankResult> MakePaymentAsync(string reference, string sortCode, string accountNumber, double amount, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(MakePaymentAsync)} Making payment of {amount:C} to account {sortCode} {accountNumber} with reference {reference}");

            return Task.FromResult(new BankResult
            {
                IsSuccess = true
            });
        }

        public Task<string> GetPaymentStatusAsync(string reference, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.ToStatus());
        }

        /*
         * Issue: Receipt printing
         * Violates: Interface Segregation Principle - mixing concerns
         * Violates: Single Responsibility Principle - UI in service
         * Violates: Dependency Inversion - direct console usage
         * Should: Move to separate service
         * Should: Use proper abstraction
         */
        public Task PrintReceiptAsync(string reference, string sortCode, string accountNumber, double amount,
            CancellationToken cancellationToken = default)
        {
            /*
             * Issue: Direct console writing in service layer
             */
            Console.WriteLine($"Receipt: PaymentRef:{reference}" +
                $" sortCode:{sortCode} accountNumber:{accountNumber} " +
                $"amount:{amount}");
            return Task.CompletedTask;
        }
    }
}