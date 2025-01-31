using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FehlerhaftPayroll.Banking;
using FehlerhaftPayroll.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FehlerhaftPayroll.Controllers
{
    /*
     * Architecture Issues:
     * - Poor dependency injection (Violates Dependency Inversion Principle - direct creation of dependencies)
     * - Missing service layer (Violates Single Responsibility Principle - controller handling business logic)
     * - Direct context access (Violates Dependency Inversion Principle - depends on concrete implementation)
     * - No error handling (Violates Robustness Principle - doesn't handle failure gracefully)
     * - Missing validation (Violates Fail-Fast Principle)
     * - Poor banking integration (Violates Interface Segregation Principle - tight coupling to banking implementation)
     */

    /*
     * Issue: Controller dependencies
     * Violates: Dependency Inversion Principle - depends on concrete FehlerhaftPayrollContext
     * Violates: Single Responsibility Principle - handles both data access and payment processing
     * Should: Inject IPaymentService, IEmployeeRepository
     */
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly FehlerhaftPayrollContext _payrollContext;
        private readonly IBankPayment _bankPayment;

        /*
         * Issue: Constructor implementation
         * Violates: Dependency Inversion Principle - creates concrete dependency
         * Violates: Hollywood Principle ("Don't call us, we'll call you")
         * Should: Inject all dependencies via DI container
         */
        public PaymentController(FehlerhaftPayrollContext payrollContext)
        {
            _payrollContext = payrollContext;
            _bankPayment = new FakePayment();
        }

        /*
         * Issue: PayEmployee method
         * Violates: Command Query Separation - mixing query and command
         * Violates: Single Responsibility Principle - handling validation, lookup, and payment
         * Violates: Tell Don't Ask Principle - querying state then making decisions
         * Should: Move to domain service
         */
        [HttpPost]
        public async Task<IActionResult> PayEmployee([FromBody] PayEmployeeRequest payEmployeeRequest)
        {
            /*
             * Issue: No input validation
             */
            var employee = await _payrollContext.Employees.SingleOrDefaultAsync(e => e.Id == payEmployeeRequest.EmployeeId);

            /*
             * Issue: Poor error response
             */
            if (employee == null)
            {
                return BadRequest();
            }

            /*
             * Issue: No transaction handling
             */
            /*
             * Issue: No retry policy
             */
            var result = _bankPayment.PayEmployee(employee, payEmployeeRequest.PaymentPeriodStart,
                payEmployeeRequest.PaymentPeriodEnd, payEmployeeRequest.PaymentReference);

            /*
             * Issue: No response type documentation
             */
            return Ok(result);
        }
    }

    /*
     * Issue: Request model
     * Violates: Encapsulation - exposes all properties as mutable
     * Violates: Input Validation principle - no validation attributes
     * Should: Add validation
     * Should: Make properties init-only
     * Should: Consider using Value Objects for dates and reference
     */
    public class PayEmployeeRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTime PaymentPeriodStart { get; set; }
        public DateTime PaymentPeriodEnd { get; set; }
        public string PaymentReference { get; set; }
    }

    /*
     * Additional Issues:
     * - No idempotency handling (Violates RESTful principles)
     * - No audit logging (Violates Accountability principle)
     * - No concurrency control (Violates Data Consistency principle)
     * - Missing proper HTTP status codes (Violates REST standards)
     * - No rate limiting for financial operations (Violates Security principle)
     * - No payment amount validation (Violates Data Integrity principle)
     */
}
