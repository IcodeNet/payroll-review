namespace FehlerhaftPayroll.Domain.Entities
{
    public class FullTimeEmployee : Employee
    {
        public FullTimeEmployee()
        {
            ContractType = ContractType.FullTime;
        }

        public override int AnnualHolidayAllowance => 25;
    }
}