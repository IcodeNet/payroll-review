using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FehlerhaftPayroll
{
    /*
     * Architecture Issues:
     * - Missing proper logging configuration (no structured logging)
     * - No application insights (missing telemetry)
     * - Missing environment handling (no proper env-based config)
     * - No startup error handling (missing global error management)
     * - Poor configuration management (no proper config validation)
     * - Missing health checks
     * - No metrics collection
     * - Missing proper hosting configuration
     * - No containerization support
     */
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
             * Issue: No startup error handling
             * Should: Add try-catch block
             * Should: Include proper logging
             * Should: Handle startup exceptions
             */
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    /*
                     * Issue: Basic console logging only
                     * Should: Add structured logging
                     * Should: Configure proper sinks
                     * Should: Include log enrichment
                     */
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    /*
                     * Issue: No environment-specific configuration
                     * Should: Add environment-based setup
                     * Should: Include proper security headers
                     * Should: Configure HTTPS properly
                     */
                    webBuilder.UseStartup<Startup>();
                });
    }
}
