using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    public class BankDetails : IEntity
    {
        public int Id { get; }

        public Guid EmployeeId { get; set; }

        public virtual Employee Employee { get; set; }

        public string SortCode { get; set; }

        public string AccountNumber { get; set; }

        public string AccountName { get; set; }
    }
}