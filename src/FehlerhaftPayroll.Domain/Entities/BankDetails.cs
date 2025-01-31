using System;

namespace FehlerhaftPayroll.Domain.Entities
{
    /*
     * Domain Issues:
     * - Poor security practices (Violates Principle of Least Privilege - sensitive data exposed)
     * - Missing data encryption (Violates Defense in Depth Principle)
     * - Weak validation rules (Violates Fail-Fast Principle)
     * - No change tracking (Violates Audit Trail Principle)
     * - Missing verification workflow (Violates Secure by Design Principle)
     * - Plain text sensitive data storage (Violates Data Protection Principle)
     * - Poor encapsulation (Violates Information Hiding Principle)
     */
    public class BankDetails : IEntity
    {
        /*
         * Issue: ID implementation
         * Violates: Encapsulation Principle - ID should be protected
         * Violates: Domain-Driven Design - entity identity should be immutable
         * Should: Use private setter or constructor assignment
         */
        public int Id { get; }

        /*
         * Issue: Employee relationship
         * Violates: Encapsulation Principle - public setter exposes internal relationship
         * Violates: Aggregate Root Pattern - direct access to relationship
         * Should: Manage relationship through Employee aggregate root
         */
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        /*
         * Issue: Bank account details
         * Violates: Data Protection Principle - sensitive data stored as plain text
         * Violates: Information Hiding Principle - direct access to sensitive fields
         * Violates: Single Responsibility Principle - class handles both storage and validation
         * Should: Use Value Objects for bank details
         * Should: Implement encryption
         * Should: Add validation rules
         */
        public string SortCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        /*
         * Missing but needed:
         * - Violates: Immutability Principle - no protection against unauthorized changes
         * - Violates: Domain Events Pattern - changes not tracked
         * - Violates: Validation Pattern - no self-validation
         * - Violates: Factory Pattern - no controlled creation
         * Should: Add validation methods
         * Should: Add factory method
         * Should: Add change tracking
         * Should: Add encryption/decryption methods
         */
    }
}