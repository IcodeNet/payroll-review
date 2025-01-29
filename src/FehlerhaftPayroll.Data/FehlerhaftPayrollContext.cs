using System.Reflection;
using FehlerhaftPayroll.Domain;
using FehlerhaftPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FehlerhaftPayroll.Data
{
    public class FehlerhaftPayrollContext : DbContext
    {
        public FehlerhaftPayrollContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Department>().HasKey(d => d.Id);
            modelBuilder.Entity<Department>().HasMany<Employee>().WithOne();

            modelBuilder.Entity<BankDetails>().ToTable("BankDetails");
            modelBuilder.Entity<BankDetails>().HasKey(bd => bd.Id);
            modelBuilder.Entity<BankDetails>().Property(bd => bd.AccountName).HasMaxLength(100);
            modelBuilder.Entity<BankDetails>().Property(bd => bd.SortCode).HasMaxLength(6);
            modelBuilder.Entity<BankDetails>().Property(bd => bd.AccountNumber).HasMaxLength(8);

            modelBuilder.Entity<Employee>()
                .HasDiscriminator<string>("employee_type")
                .HasValue<Contractor>("contractor")
                .HasValue<FullTimeEmployee>("full-time")
                .HasValue<PartTimeEmployee>("part-time");


        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}