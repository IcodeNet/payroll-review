using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FehlerhaftPayroll.Data;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace FehlerhaftPayroll.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly FehlerhaftPayrollContext _payrollContext;

        public EmployeeController(FehlerhaftPayrollContext payrollContext)
        {
            _payrollContext = payrollContext;
        }

        [HttpGet]
        public async Task<IActionResult> ListEmployees()
        {
            var response = new EmployeesByDepartmentResponse();
            
            // get employees
            var departments = await _payrollContext.Departments.ToListAsync();

            foreach (var department in departments)
            {
                var departmentResponse = new DepartmentResponse();
                response.Departments.Add(departmentResponse);
                
                var employees = department.Employees;

                foreach(var employee in employees)
                {
                    departmentResponse.Employees.Add(employee);
                }
            }

            return Ok(response);
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateEmployee([FromBody] NewEmployeeModel newEmployee,
            CancellationToken cancellationToken)
        {
            var department =
                await _payrollContext.Departments.SingleOrDefaultAsync(d => d.DepartmentName == newEmployee.Department,
                    cancellationToken);

            EntityEntry<Employee> employeeEntity = null;

            switch (newEmployee.ContractType)
            {
                case ContractType.FullTime:
                    employeeEntity =
                        await _payrollContext.Employees.AddAsync(new FullTimeEmployee {Name = newEmployee.Name},
                            cancellationToken);
                    break;

                case ContractType.Contractor:
                    employeeEntity = await _payrollContext.Employees.AddAsync(new Contractor {Name = newEmployee.Name},
                        cancellationToken);
                    break;

                case ContractType.PartTime:
                    employeeEntity =
                        await _payrollContext.Employees.AddAsync(new PartTimeEmployee {Name = newEmployee.Name},
                            cancellationToken);
                    break;

                default:
                    return NotFound();
            }

            await _payrollContext.SaveChangesAsync(cancellationToken);

            return Ok(employeeEntity.Entity.Id);
        }

        [HttpPost("{employeeId}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid employeeId,
            [FromBody] UpdateEmployeeRequest request,
            CancellationToken cancellationToken)
        {

            var employee =
                await _payrollContext.Employees.SingleOrDefaultAsync(e => e.Id == employeeId, CancellationToken.None);

            if (employee == null)
            {
                return NotFound();
            }

            employee.UpdateEmployee(request.Name, request.HolidayAllowance);
            employee.BankDetails.AccountNumber = request.BankAccountNumber;
            employee.BankDetails.SortCode = request.SortCode;
            employee.AnnualPay = request.AnnualPay.Value;

            switch (employee.ContractType)
            {
                case ContractType.Contractor:
                    var contractor = employee as Contractor;
                    contractor.BankDetails.AccountNumber = request.BankAccountNumber;
                    contractor.ContractExpires =
                        request.ContractExpiryDate.GetValueOrDefault(contractor.ContractExpires);
                    break;
            }

            await _payrollContext.SaveChangesAsync(cancellationToken);

            return Accepted();
        }
    }

    public class NewEmployeeModel
    {
        public string Department { get; init; }
        public string Name { get; init; }

        public ContractType ContractType { get; init; }

    }
    
    public class UpdateEmployeeRequest
    {
        public string? Name { get; init; }

        public int? HolidayAllowance { get; init; }

        public DateTime? ContractExpiryDate { get; init; }
        public string? BankAccountNumber { get; set; }
        public string? SortCode { get; set; }

        public decimal? AnnualPay { get; set; }
    }

    public class EmployeesByDepartmentResponse
    {
        public IList<DepartmentResponse> Departments { get; } = new List<DepartmentResponse>();

        public int TotalEmployees => Departments.Sum(dr => dr.Employees.Count);

    }

    public class DepartmentResponse
    {
        public string DepartmentName { get; set; }
        public IList<Employee> Employees { get; } = new List<Employee>();

    }
}