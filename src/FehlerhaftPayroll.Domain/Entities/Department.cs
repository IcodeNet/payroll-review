using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Poor encapsulation (Violates Information Hiding Principle)
     * - Missing hierarchy validation (Violates Domain Invariants Principle)
     * - No capacity limits (Violates Business Rules Encapsulation)
     * - Missing structure rules (Violates Domain Integrity Principle)
     * - No budget tracking (Violates Domain Completeness Principle)
     * - Weak naming rules (Violates Value Object Pattern)
     */
    public class Department : IAggregate
    {
        /*
         * Issue: ID implementation
         * Violates: Encapsulation Principle - public setter
         * Violates: Identity Pattern - mutable identity
         * Should: Make ID immutable
         * Should: Use constructor assignment
         */
        public int Id { get; set; }

        /*
         * Issue: Department name
         * Violates: Value Object Pattern - should be value object
         * Violates: Domain Validation - database-only validation
         * Should: Create DepartmentName value object
         * Should: Add domain validation
         */
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        /*
         * Issue: Employee collection
         * Violates: Collection Encapsulation Principle
         * Violates: Aggregate Root Pattern - direct collection access
         * Violates: Domain Events - changes not tracked
         * Should: Encapsulate collection
         * Should: Add methods for employee management
         * Should: Raise events on changes
         */
        public IList<Employee> Employees { get; set; } = new List<Employee>();
    }
}
