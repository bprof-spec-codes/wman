using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Wman.Data;
using Wman.Data.DB_Models;
using Wman.Logic.Classes;
using Wman.Logic.Helpers;
using Wman.Logic.Interfaces;
using Wman.Logic.Services;
using Wman.Repository.Classes;
using Wman.Repository.Interfaces;
using Wman.WebAPI.Helpers;
//using System.Data.Entity.Database;

namespace Wman.WebAPI
{
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
            string signingKey = Configuration.GetValue<string>("SigningKey");
            services.AddSingleton<NotifyHub>();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddControllers(x => x.Filters.Add(new ApiExceptionFilter()));
            services.AddTransient<IAuthLogic, AuthLogic>();
            services.AddTransient<ICalendarEventLogic, CalendarEventLogic>();
            services.AddTransient<IEventLogic, EventLogic>();
            services.AddTransient<IUserLogic, UserLogic>();
            services.AddTransient<DBSeed, DBSeed>();
            services.AddTransient<ILabelLogic, LabelLogic>();
            services.AddTransient<IAllInWorkEventLogic, AllInWorkEventLogic>();
            services.AddTransient<IPhotoLogic, PhotoLogic>();
            services.AddTransient<IAdminLogic, AdminLogic>();
            services.AddTransient<IStatsLogic, StatsLogic>();
            services.AddControllers().AddJsonOptions(options =>
          options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            //services.AddSingleton(Configuration);
#if DEBUG
            //services.AddSingleton<IAuthorizationHandler, AllowAnonymous>(); //Uncommenting this will disable auth, for debugging purposes.
#endif

            services.AddTransient<IProofOfWorkRepo, ProofOfWorkRepo>();
            services.AddTransient<IWorkEventRepo, WorkEventRepo>();
            services.AddTransient<IPicturesRepo, PicturesRepo>();
            services.AddTransient<ILabelRepo, LabelRepo>();
            services.AddTransient<IAddressRepo, AddressRepo>();
            services.AddTransient<IFileRepo, FileRepo>();
            services.AddTransient<IPhotoService, PhotoService>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddSwaggerGen(c =>
            {
                //c.DescribeAllEnumsAsStrings();
                // configure SwaggerDoc and others
                //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wman.WebAPI", Version = "v1" });
                // add JWT Authentication
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.IncludeXmlComments(XmlCommentsFilePath);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {securityScheme, new string[] { }}
                });
            });
            string appsettingsConnectionString = Configuration.GetConnectionString("wmandb");

            services.AddDbContext<wmanDb>(options => options
#if DEBUG
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
#else

#endif
            .UseSqlServer(appsettingsConnectionString, b => b.MigrationsAssembly("Wman.WebAPI")));

            services.AddIdentityCore<WmanUser>(
                     option =>
                     {
                         option.Password.RequireDigit = false;
                         option.Password.RequiredLength = 6;
                         option.Password.RequireNonAlphanumeric = false;
                         option.Password.RequireUppercase = false;
                         option.Password.RequireLowercase = false;
                     }
                 ).AddRoles<IdentityRole<int>>()
                 .AddRoleManager<RoleManager<IdentityRole<int>>>()
                 .AddSignInManager<SignInManager<WmanUser>>()
                 .AddRoleValidator<RoleValidator<IdentityRole<int>>>()
                 .AddEntityFrameworkStores<wmanDb>()
                 .AddDefaultTokenProviders();


            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = "http://www.security.org",
                    ValidIssuer = "http://www.security.org",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/notify")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                                  });
            });

            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddSignalR();
            services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(Configuration.GetConnectionString("wmandb"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStatsLogic statsLogic)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
#if DEBUG
            app.UseHangfireDashboard();
#endif
            app.UseSwagger();
            //app.UseStatusCodePages();
            app.UseSwaggerUI(c =>
            {

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wman.WebAPI v1");

            });
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
#if DEBUG
                endpoints.MapHangfireDashboard();
#endif
                endpoints.MapHub<NotifyHub>("/notify", opt =>
                {

                });
            });
            statsLogic.registerRecurringManagerJob(Configuration.GetValue<string>("xlsSchedule"));
            statsLogic.registerRecurringWorkerJob(Configuration.GetValue<string>("xlsSchedule_Worker"));
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}
