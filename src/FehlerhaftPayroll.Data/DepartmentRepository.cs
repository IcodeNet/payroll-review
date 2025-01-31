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
    /*
     * Architecture Issues:
     * - Missing repository interface (Violates Dependency Inversion Principle)
     * - No unit of work pattern (Violates Unit of Work Pattern - missing atomic operations)
     * - Poor error handling (Violates Robustness Principle)
     * - Missing async support (Violates Asynchronous Programming Principles)
     * - No caching strategy (Violates Performance Optimization Principle)
     * - Missing specification pattern (Violates Query Object Pattern)
     * - No audit logging (Violates Audit Trail Pattern)
     * - Missing optimistic concurrency (Violates Data Consistency Principle)
     * - No soft delete implementation (Violates Data Lifecycle Pattern)
     */
    public class DepartmentRepository
    {
        /*
         * Issue: Repository implementation
         * Violates: Dependency Inversion Principle - concrete dependency
         * Violates: Interface Segregation Principle - no abstraction
         * Violates: Repository Pattern - incomplete implementation
         * Should: Implement IRepository<Department>
         * Should: Use IUnitOfWork
         */
        private readonly FehlerhaftPayrollContext _payrollContext;

        /*
         * Issue: Constructor
         * Violates: Dependency Inversion Principle - concrete dependency
         * Violates: Guard Clause Pattern - no parameter validation
         * Should: Depend on abstractions
         * Should: Validate dependencies
         */
        public DepartmentRepository(FehlerhaftPayrollContext payrollContext)
        {
            _payrollContext = payrollContext;
        }

        /*
         * Issue: Method implementation
         * Violates: Command Query Separation - mixing sync/async
         * Violates: Error Handling Pattern - no proper error handling
         * Violates: Null Object Pattern - returns null
         * Violates: Repository Pattern - incomplete implementation
         * Should: Use async/await consistently
         * Should: Return Result<T>
         * Should: Add proper error handling
         */
        public Department GetDepartmentByName(string name, CancellationToken cancellationToken)
        {
            /*
             * Issue: No validation
             * Issue: No null checks
             * Issue: Missing proper async/await
             * Should: Validate input parameters
             * Should: Use async operation
             * Should: Include proper error responses
             * Should: Add caching layer
             */
            return _payrollContext.Departments.SingleOrDefault(d => d.DepartmentName == name);
        }
    }
}
