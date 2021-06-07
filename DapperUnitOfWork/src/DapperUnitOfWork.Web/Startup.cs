using System.Text.Json.Serialization;
using DapperUnitOfWork.Application;
using DapperUnitOfWork.Application.Seedwork.Interfaces;
using DapperUnitOfWork.Infrastructure;
using DapperUnitOfWork.Web.Seedwork;
using DapperUnitOfWork.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;

namespace DapperUnitOfWork.Web
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure(Configuration);     
            services.AddSingleton<IIdentityService, IdentityService>();
            
            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(typeof(WebExceptionFilterAttribute));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddOpenApiDocument(generatorSettings =>
            {
                generatorSettings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "DapperUnitOfWork API";
                    document.Info.Description = "Tests out Dapper and Unit of Work";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "John Doe",
                        Email = "mail@bugfree.com",
                        Url = "https://bugfree.dk"
                    };
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();
            app.UseMvcWithDefaultRoute();
        }
    }
}
