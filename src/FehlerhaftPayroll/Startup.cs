using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FehlerhaftPayroll.Data;
using Microsoft.EntityFrameworkCore;

namespace FehlerhaftPayroll
{
    /*
     * Architecture Issues:
     * - Missing proper DI configuration
     * - Hard-coded connection string
     * - Missing middleware configuration
     * - Poor error handling setup
     * - No authentication configuration
     * - Missing proper logging setup
     * - No health checks
     */
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Issue: Direct connection string access without validation
            var connectionString = Configuration.GetConnectionString("FehlerhaftPayrollContext");

            // Issue: Missing DbContext configuration options
            services.AddDbContext<FehlerhaftPayrollContext>(options =>
                options.UseSqlServer(connectionString));

            // Issue: Missing proper service registration
            services.AddScoped<DepartmentRepository>();

            // Issue: Missing authentication setup
            // Issue: Missing authorization policies
            services.AddControllers();

            // Issue: Basic Swagger setup without security
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FehlerhaftPayroll", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Issue: Always showing developer exception page
            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                // Issue: Swagger enabled without security
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FehlerhaftPayroll v1"));

                // Issue: Direct database migration in startup
                using var serviceScope = app.ApplicationServices.CreateScope();
                var services = serviceScope.ServiceProvider;
                var dbContext = services.GetService<FehlerhaftPayrollContext>();
                dbContext.Database.Migrate();
            }

            // Issue: Missing proper middleware order
            app.UseHttpsRedirection();
            app.UseRouting();
            // Issue: Authorization without authentication
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
