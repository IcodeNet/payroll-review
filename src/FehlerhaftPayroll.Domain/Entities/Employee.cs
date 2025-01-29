using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    public class Employee : IAggregate<Guid>
    {
        public const int Default_Holiday_Allowance_Days = 25;
     
        public Guid Id { get; set; }

        public ContractType ContractType { get; set; }

        public virtual int AnnualHolidayAllowance { get; set; } = Default_Holiday_Allowance_Days;

        public string Name { get; set; }

        public BankDetails BankDetails { get; set; }

        public decimal AnnualPay { get; set; }

        public virtual void UpdateEmployee(string name, int? holidayAllowance)
        {
            Name = name ?? Name;
            AnnualHolidayAllowance = holidayAllowance.GetValueOrDefault(AnnualHolidayAllowance);
        }
    }
}
