using Baitkm.Application.ApplicationStartup;
using Baitkm.Application.Middlewares.AuthorizationHandler;
using Baitkm.Application.Middlewares.IpAddress;
using Baitkm.Application.Middlewares.Language;
using Baitkm.Application.Middlewares.Managers;
using Baitkm.Application.Middlewares.Socket;
using Baitkm.Application.Middlewares.Statistics;
using Baitkm.BLL.Services;
using Baitkm.BLL.Services.Emails;
using Baitkm.DAL.Context;
using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Configurations;
using Baitkm.Extensions;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers.Binders;
using Baitkm.Infrastructure.Helpers.Models;
using Baitkm.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Baitkm
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            var isDevelopment = false;
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
                isDevelopment = true;
            }
            IsDevelopment = isDevelopment;
            builder.AddEnvironmentVariables();
            EnvName = env.EnvironmentName;
            Configuration = builder.Build();
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public string EnvName { get; set; }
        public IConfiguration Configuration { get; }
        public bool IsDevelopment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IBaitkmDbContext, BaitkmDbContext>();
            services.AddWebSockets(options => { });


            //services.AddScoped<IEmailService, EmailService>();
            //var s = services.Configure<NgrokSettings>(options => Configuration.GetSection("NgrokSettings"));
            services.Configure<ErrorMessagesEnglish>(options => Configuration.GetSection("ErrorMessageEnglish").Bind(options));
            services.Configure<ErrorMessageArabian>(options => Configuration.GetSection("ErrorMessageArabian").Bind(options));
            services.Configure<Currencies>(Configuration.GetSection("Currencies"));
            services.Configure<TestDic>(Configuration.GetSection("Currencies"));
            services.Configure<CurrnecySymbols>(options => Configuration.GetSection("CurrnecySymbols").Bind(options));

            services.ConfigureDb(Configuration);
            services.ConfigureServices();
            services.ConfigureSwagger();
            services.ConfigureJwtToken();
            services.ConfigureValidation();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 6000000000;
            });

            var secretKey = Configuration.GetSection("AppSettings:SecretKey").Value;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            Configuration.GetSection("ConstValues").Get<ConstValues>();
            ConstValues.IsDevelopmentEnvironment = IsDevelopment;
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            Configuration.GetSection("AppSettings").Get<AppSettings>();
            Configuration.GetSection("QuartzSettings").Get<QuartzSettings>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvcCore().AddAuthorization().AddJsonOptions(options =>
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            services.AddAuthorization(o =>
            {
                o.AddPolicy("PolicyName", p =>
                {
                    p.RequireRole("Roles");
                    p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireAuthenticatedUser();
                });
                o.AddPolicy("CheckUnauthorized", p =>
                {
                    p.Requirements.Add(new AuthorizeRequirement());
                });
            });

            var nameValue = new NameValueCollection();
            var properties = typeof(QuartzSettings).GetProperties();
            foreach (var variable in properties)
            {
                var attributeValue = variable.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType == typeof(DisplayNameAttribute))?.ConstructorArguments
                    .FirstOrDefault().Value.ToString();
                var value = variable.GetValue(attributeValue).ToString();
                if (string.IsNullOrEmpty(attributeValue))
                    continue;
                nameValue.Add(attributeValue, value);
            }
            ConstValues.QuartzConfigs = nameValue;

            services.AddHttpClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime,
             IConfiguration configuration, IOptionsBinder optionsBinder)
        {
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
               
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var quartz = new SchedulerStartup();
            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopping.Register(quartz.Stop);


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMiddleware<LanguageMiddleware>(optionsBinder);

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                var rM = context.RequestServices.GetService<IRoleManager>();
                var aL = context.RequestServices.GetService<IActivityLogger>();
                var iP = context.RequestServices.GetService<ILocateByIpAddress>();
                var userName = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var verifiedBy = context.User.Claims.Where(c => c.Type == "verifiedBy").Select(c => c.Value).FirstOrDefault();
                if (string.IsNullOrEmpty(userName))
                    await next.Invoke();
                else
                {
                    await aL.Log(userName);
                    var ipAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    await iP.Locate(userName, ipAddress, verifiedBy);
                    var success = await rM.CheckValidity(userName, verifiedBy);
                    if (!success)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await next.Invoke();
                    }
                    else
                        await next.Invoke();
                }
            });
            app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                    await next.Invoke();
                else
                {
                    var repo = context.RequestServices.GetService<IEntityRepository>();
                    await context.Map(repo);
                }
            });
            app.UseMvcWithDefaultRoute();
        }
    }
}