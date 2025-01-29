using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FehlerhaftPayroll.Domain.Entities
{
    public class Department : IAggregate
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string DepartmentName { get; set; }

        public IList<Employee> Employees { get; set; } = new List<Employee>();
    }
}
