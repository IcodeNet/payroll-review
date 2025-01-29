using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    public class Contractor : Employee
    {
        public Contractor()
        {
            ContractType = ContractType.Contractor;
        }

        #region "properties"
        public DateTime ContractExpires { get; set; }
        #endregion
        
        #region "compound properties"
        public override int AnnualHolidayAllowance => throw new ArgumentOutOfRangeException($"No holiday allowance for contractors");
        #endregion
    }
}