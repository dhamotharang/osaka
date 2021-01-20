using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HappyTravel.LocationService.Infrastructure;
using HappyTravel.LocationService.Infrastructure.Extensions;
using HappyTravel.LocationService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace HappyTravel.LocationService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }
        
        
        public void ConfigureServices(IServiceCollection services)
        {
            using var vaultClient = VaultHelper.CreateVaultClient(_configuration);
            var token = _configuration[_configuration["Vault:Token"]];
            vaultClient.Login(token).GetAwaiter().GetResult();
            
            services.AddTransient<IPredictionsService, PredictionsService>();

            services.AddResponseCompression()
                .AddCors()
                .AddLocalization()
                .AddTracing(_hostEnvironment, _configuration);
            
            services.ConfigureAuthentication(_configuration, _hostEnvironment, vaultClient);
            services.AddHealthChecks()
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));
            
            services.AddProblemDetailsErrorHandling();
            
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            
            services.AddElasticsearchClient(_configuration, vaultClient);
            
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new OpenApiInfo {Title = "HappyTravel.com Location Service API", Version = "v1.0"});

                var xmlCommentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFileName);
                options.CustomSchemaIds(t => t.FullName);
                options.IncludeXmlComments(xmlCommentsFilePath);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddMvcCore()
                .AddAuthorization()
                .AddControllersAsServices()
                .AddMvcOptions(m => m.EnableEndpointRouting = true)
                .AddFormatterMappings()
                .AddApiExplorer()
                .AddCacheTagHelper()
                .AddDataAnnotations();
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            app.UseProblemDetailsExceptionHandler(env, logger);

            app.UseHttpContextLogging(
                options => options.IgnoredPaths = new HashSet<string> {"/health"}
            );

            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "HappyTravel.com Location Service API");
                    options.RoutePrefix = string.Empty;
                });

            app.UseResponseCompression()
                .UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            
            app.UseHealthChecks("/health");
            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;
    }
}