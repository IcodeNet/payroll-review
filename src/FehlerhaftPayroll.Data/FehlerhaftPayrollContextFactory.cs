using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FehlerhaftPayroll.Data 
{ 
    public class FehlerhaftPayrollContextFactory : IDesignTimeDbContextFactory<FehlerhaftPayrollContext>
    {
        public FehlerhaftPayrollContext CreateDbContext(string[] args)
        {
            string connectionString = @"server=(localdb)\mssqllocaldb;database=FehlerhaftPayroll;trusted_connection=yes;";

            return new FehlerhaftPayrollContext(new DbContextOptionsBuilder<FehlerhaftPayrollContext>()
                .UseSqlServer(connectionString)
                .Options);
        }
    }
}
