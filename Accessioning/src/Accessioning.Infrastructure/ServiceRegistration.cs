namespace Accessioning.Infrastructure
{
    using Accessioning.Infrastructure.Contexts;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sieve.Services;

    public static class ServiceRegistration
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            // DbContext -- Do Not Delete          
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<AccessioningDbContext>(options =>
                    options.UseInMemoryDatabase($"AccessioningDbContext"));
            }
            else
            {
                services.AddDbContext<AccessioningDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("AccessioningDbContext"),
                        builder => builder.MigrationsAssembly(typeof(AccessioningDbContext).Assembly.FullName)));
            }

            services.AddScoped<SieveProcessor>();

            // Auth -- Do Not Delete
            if(env.EnvironmentName != "IntegrationTesting")
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = configuration["JwtSettings:Authority"];
                        options.Audience = configuration["JwtSettings:Audience"];
                    });
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanReadPatients", 
                    policy => policy.RequireClaim("scope", "patients.read"));
                options.AddPolicy("CanAddPatients", 
                    policy => policy.RequireClaim("scope", "patients.add"));
                options.AddPolicy("CanDeletePatients", 
                    policy => policy.RequireClaim("scope", "patients.delete"));
                options.AddPolicy("CanUpdatePatients", 
                    policy => policy.RequireClaim("scope", "patients.update"));
            });
        }
    }
}
