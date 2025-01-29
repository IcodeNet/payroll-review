namespace FehlerhaftPayroll.Domain.Entities
{
    public class PartTimeEmployee : Employee
    {
        public PartTimeEmployee()
        {
            ContractType = ContractType.PartTime;
        }

        public override int AnnualHolidayAllowance => 12;
    }
}