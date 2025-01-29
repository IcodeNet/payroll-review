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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly FehlerhaftPayrollContext _payrollContext;
        private readonly IBankPayment _bankPayment;

        public PaymentController(FehlerhaftPayrollContext payrollContext, IBankPayment bankPayment)
        {
            _payrollContext = payrollContext;
            _bankPayment = new FakePayment();
        }

        [HttpPatch]
        public async Task<IActionResult> PayEmployee([FromBody] PayEmployeeRequest payEmployeeRequest)
        {
            var employee = await _payrollContext.Employees.SingleOrDefaultAsync(e => e.Id == payEmployeeRequest.EmployeeId);

            if (employee == null)
            {
                return BadRequest();
            }

            var result = _bankPayment.PayEmployee(employee, payEmployeeRequest.PaymentPeriodStart,
                payEmployeeRequest.PaymentPeriodEnd, payEmployeeRequest.PaymentReference);

            return Ok(result);
        }
    }

    public class PayEmployeeRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTime PaymentPeriodStart { get; set; }
        public DateTime PaymentPeriodEnd { get; set; }
        public string PaymentReference { get; set; }
    }
}
