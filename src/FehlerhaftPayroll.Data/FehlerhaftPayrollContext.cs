using System.Reflection;
using FehlerhaftPayroll.Domain;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FehlerhaftPayroll.Data
{
    /*
     * Architecture Issues:
     * - Missing proper entity configurations (Violates Single Responsibility Principle)
     * - Poor separation of configurations (Violates Separation of Concerns)
     * - Hard-coded relationships (Violates Configuration Pattern)
     * - Missing audit fields (Violates Audit Trail Pattern)
     * - No soft delete support (Violates Data Lifecycle Pattern)
     * - Missing proper indexing strategy (Violates Performance Optimization)
     * - Poor inheritance mapping (Violates Object-Relational Mapping Principles)
     */
    public class FehlerhaftPayrollContext : DbContext
    {
        public FehlerhaftPayrollContext(DbContextOptions options) : base(options)
        {
        }

        /*
         * Issue: Model configuration
         * Violates: Single Responsibility Principle - all config in one place
         * Violates: Open/Closed Principle - hard to extend
         * Should: Use separate configuration classes
         * Should: Follow fluent configuration pattern
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * Issue: Entity configurations
             * Violates: Separation of Concerns - mixed configurations
             * Violates: Configuration Pattern - hard-coded values
             * Violates: DRY Principle - repeated configuration code
             * Should: Use IEntityTypeConfiguration<T>
             * Should: Move to separate files
             */
            modelBuilder.Entity<Department>().HasKey(d => d.Id);

            /*
             * Issue: Relationship mapping
             * Violates: Fluent Interface Pattern - incomplete configuration
             * Violates: Domain-Driven Design - relationships not explicit
             * Should: Use complete relationship configuration
             * Should: Follow domain model relationships
             */
            modelBuilder.Entity<Department>().HasMany<Employee>().WithOne();

            /*
             * Issue: Value object mapping
             * Violates: Value Object Pattern - treated as entity
             * Violates: Domain-Driven Design - missing proper mapping
             * Should: Use value object configuration
             * Should: Map to appropriate database structure
             */
            modelBuilder.Entity<BankDetails>().ToTable("BankDetails");

            /*
             * Issue: Inheritance strategy
             * Violates: Object-Relational Mapping Principles
             * Violates: Domain Model Purity - mixing persistence concerns
             * Should: Consider Table-per-Type strategy
             * Should: Separate persistence concerns
             */
            modelBuilder.Entity<Employee>()
                .HasDiscriminator<string>("employee_type");
        }

        /*
         * Issue: DbSet properties
         * Violates: Repository Pattern - direct access to sets
         * Violates: Encapsulation - exposed persistence details
         * Should: Use repository pattern
         * Should: Hide DbSet properties
         */
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*
             * Issue: Missing proper configuration handling
             */
            base.OnConfiguring(optionsBuilder);
        }
    }
}