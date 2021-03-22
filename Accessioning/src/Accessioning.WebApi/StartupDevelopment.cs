namespace Accessioning.WebApi
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Accessioning.Infrastructure;
    using Accessioning.Infrastructure.Seeders;
    using Accessioning.Infrastructure.Contexts;
    using Accessioning.WebApi.Extensions;
    using Serilog;

    public class StartupDevelopment
    {
        public IConfiguration _config { get; }
        public IWebHostEnvironment _env { get; }

        public StartupDevelopment(IConfiguration configuration, IWebHostEnvironment env)
        {
            _config = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorsService("MyCorsPolicy");
            services.AddInfrastructure(_config, _env);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddWebApiServices();
            services.AddHealthChecks();

            #region Dynamic Services
            services.AddSwaggerExtension(_config);
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            #region Entity Context Region - Do Not Delete

                using (var context = app.ApplicationServices.GetService<AccessioningDbContext>())
                {
                    context.Database.EnsureCreated();

                    #region AccessioningDbContext Seeder Region - Do Not Delete
                    
                    PatientSeeder.SeedSamplePatientData(app.ApplicationServices.GetService<AccessioningDbContext>());
                    SampleSeeder.SeedSampleSampleData(app.ApplicationServices.GetService<AccessioningDbContext>());
                    #endregion
                }

            #endregion

            app.UseCors("MyCorsPolicy");

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseErrorHandlingMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/health");
                endpoints.MapControllers();
            });

            #region Dynamic App
            app.UseSwaggerExtension(_config);
            #endregion
        }
    }
}
