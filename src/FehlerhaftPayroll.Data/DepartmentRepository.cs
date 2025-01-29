using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FehlerhaftPayroll.Data
{
    public class DepartmentRepository
    {
        private readonly FehlerhaftPayrollContext _payrollContext;

        public DepartmentRepository(FehlerhaftPayrollContext payrollContext)
        {
            _payrollContext = payrollContext;
        }

        public Department GetDepartmentByName(string name, CancellationToken cancellationToken)
        {
            return _payrollContext.Departments.SingleOrDefault(d => d.DepartmentName == name);
        }
    }
}
