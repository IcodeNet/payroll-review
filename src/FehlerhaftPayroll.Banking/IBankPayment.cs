using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FehlerhaftPayroll.Domain.Entities;

namespace FehlerhaftPayroll.Banking
{
    public interface IBankPayment
    {
        Task<BankResult> MakePaymentAsync(string reference, string sortCode, string accountNumber, double amount, CancellationToken cancellationToken = default);
        Task<string> GetPaymentStatusAsync(string reference, CancellationToken cancellationToken = default);
        Task PrintReceiptAsync(string reference, string sortCode, string accountNumber, double amount, CancellationToken cancellationToken = default);
        BankResult PayEmployee(Employee employee, DateTime periodStart, DateTime periodEnd, string reference);
    }   
}
