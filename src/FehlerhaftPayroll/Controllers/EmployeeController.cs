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
    /*
     * Architecture Issues:
     * - Missing service layer abstraction (controller doing too much work)
     * - Business logic scattered in controllers (should be in domain/services)
     * - Direct repository/context access (violates dependency inversion)
     * - Poor separation of concerns (mixing HTTP, business logic, data access)
     * - Missing validation layer (no input validation strategy)
     * - No error handling strategy (missing global error handling)
     * - Missing authentication (security concern)
     * - No logging strategy (operations not tracked)
     * - No caching strategy
     * - Missing proper response types
     * - No rate limiting
     */
    //[Authorize]  // Issue: Authentication commented out
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        /*
         * Issue: Direct context dependency
         * Should: Inject IEmployeeService interface
         * Should: Use repository pattern
         * Should: Add unit of work pattern for transaction management
         */
        private readonly FehlerhaftPayrollContext _payrollContext;

        /*
         * Issue: Constructor only injects data context
         * Should: Inject required services via constructor
         * Should: Validate injected dependencies
         * Should: Consider adding logging service
         */
        public EmployeeController(FehlerhaftPayrollContext payrollContext)
        {
            _payrollContext = payrollContext;
        }

        /*
         * Issue: No API versioning
         * Issue: No response type attributes
         * Should: Add [ProducesResponseType] attributes
         * Should: Add API versioning
         * Should: Add response caching headers
         * Should: Add rate limiting attributes
         */
        [HttpGet]
        public async Task<IActionResult> ListEmployees()
        {
            /*
             * Issue: N+1 query problem
             * Should: Use proper eager loading
             * Should: Implement pagination
             * Should: Add caching strategy
             */
            var response = new EmployeesByDepartmentResponse();

            /*
             * Issue: Loading all records without pagination
             * Should: Implement proper pagination
             * Should: Add filtering options
             * Should: Consider performance implications
             */
            var departments = await _payrollContext.Departments.ToListAsync();

            foreach (var department in departments)
            {
                // Issue: Inefficient data loading
                var departmentResponse = new DepartmentResponse();
                response.Departments.Add(departmentResponse);

                // Issue: Potential N+1 query
                var employees = department.Employees;

                // Issue: No error handling
                foreach (var employee in employees)
                {
                    departmentResponse.Employees.Add(employee);
                }
            }

            return Ok(response);
        }

        /*
         * Issue: Inconsistent route naming ("add" vs REST conventions)
         * Issue: No input validation
         * Issue: No proper error handling
         * Should: Use REST conventional routes
         * Should: Add model validation attributes
         * Should: Return proper HTTP status codes
         * Should: Add logging
         */
        [HttpPost("add")]
        public async Task<IActionResult> CreateEmployee([FromBody] NewEmployeeModel newEmployee,
            CancellationToken cancellationToken)
        {
            /*
             * Issue: No validation of department existence before use
             * Issue: Switch statement violates Open/Closed principle
             * Should: Move employee creation to factory pattern
             * Should: Add proper error handling
             * Should: Validate department exists
             * Should: Use domain events for side effects
             */
            var department =
                await _payrollContext.Departments.SingleOrDefaultAsync(d => d.DepartmentName == newEmployee.Department,
                    cancellationToken);

            EntityEntry<Employee> employeeEntity = null;

            /*
             * Issue: Switch statement in CreateEmployee
             * Violates: Open/Closed Principle - need to modify code to add new employee types
             * Violates: Single Responsibility Principle - mixing creation logic with HTTP concerns
             */
            switch (newEmployee.ContractType)
            {
                case ContractType.FullTime:
                    employeeEntity =
                        await _payrollContext.Employees.AddAsync(new FullTimeEmployee { Name = newEmployee.Name },
                            cancellationToken);
                    break;

                case ContractType.Contractor:
                    employeeEntity = await _payrollContext.Employees.AddAsync(new Contractor { Name = newEmployee.Name },
                        cancellationToken);
                    break;

                case ContractType.PartTime:
                    employeeEntity =
                        await _payrollContext.Employees.AddAsync(new PartTimeEmployee { Name = newEmployee.Name },
                            cancellationToken);
                    break;

                default:
                    return NotFound();
            }

            await _payrollContext.SaveChangesAsync(cancellationToken);

            return Ok(employeeEntity.Entity.Id);
        }

        /*
         * Issue: PUT would be more appropriate than POST for updates
         * Issue: No concurrency handling
         * Issue: Mixed responsibility in update logic
         * Should: Use PUT verb
         * Should: Add ETag/concurrency token
         * Should: Move update logic to domain service
         * Should: Validate all inputs
         */
        [HttpPost("{employeeId}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid employeeId,
            [FromBody] UpdateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            /*
             * Issue: UpdateEmployee method
             * Violates: Command Query Separation - mixing query and command
             * Violates: Tell Don't Ask Principle - getting state then making decisions
             * Violates: Single Responsibility Principle - handling multiple types of updates
             */
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

    /*
     * Issue: Model lacks validation attributes
     * Issue: No documentation attributes
     * Should: Add data annotations
     * Should: Add XML documentation
     * Should: Consider using record types
     * Should: Add custom validation rules
     */
    public class NewEmployeeModel
    {
        public string Department { get; init; }
        public string Name { get; init; }

        public ContractType ContractType { get; init; }

    }

    /*
     * Issue: Nullable properties without validation
     * Issue: Mixed concerns (bank details + contract details)
     * Should: Split into separate request models
     * Should: Add validation attributes
     * Should: Consider using value objects
     * Should: Add business rule validation
     */
    public class UpdateEmployeeRequest
    {
        public string? Name { get; init; }

        public int? HolidayAllowance { get; init; }

        public DateTime? ContractExpiryDate { get; init; }
        public string? BankAccountNumber { get; set; }
        public string? SortCode { get; set; }

        public decimal? AnnualPay { get; set; }
    }

    /*
     * Issue: Response model tightly coupled to domain
     * Issue: No pagination metadata
     * Should: Use DTOs instead of domain entities
     * Should: Add pagination metadata
     * Should: Add hypermedia links
     * Should: Consider using JsonApiSerializer
     */
    public class EmployeesByDepartmentResponse
    {
        public IList<DepartmentResponse> Departments { get; } = new List<DepartmentResponse>();

        public int TotalEmployees => Departments.Sum(dr => dr.Employees.Count);

    }

    /*
     * Issue: Exposes domain entities directly
     * Issue: Missing documentation
     * Should: Use DTOs
     * Should: Add documentation
     * Should: Consider adding department statistics
     * Should: Add resource links
     */
    public class DepartmentResponse
    {
        public string DepartmentName { get; set; }
        public IList<Employee> Employees { get; } = new List<Employee>();

    }
}