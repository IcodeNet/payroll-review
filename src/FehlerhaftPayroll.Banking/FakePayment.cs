using System;
using System.Threading;
using System.Threading.Tasks;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FehlerhaftPayroll.Banking
{
    public class FakePayment : IBankPayment
    {
        private readonly ILogger<FakePayment> _logger;

        public FakePayment()
        {
            _logger = new Logger<FakePayment>(new NullLoggerFactory());
        }

        public BankResult PayEmployee(Employee employee, DateTime periodStart, DateTime periodEnd, string paymentReference)
        {
            var paymentPeriodDays = periodEnd - periodStart;
            var amountToPay = (double)(employee.AnnualPay * paymentPeriodDays.Days);

            _logger.LogInformation($"Paying {employee.Name}, {amountToPay:C} for the period {periodStart} to {periodEnd}");
            
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

        public Task PrintReceiptAsync(string reference, string sortCode, string accountNumber, double amount,
            CancellationToken cancellationToken = default)
        {
    Console
.WriteLine($"Receipt: PaymentRef:{reference}" +
        $" sortCode:{sortCode} accountNumber:{accountNumber} " +
                                                                                            $"amount:{amount}");
            return Task.CompletedTask;
        }
    }
}