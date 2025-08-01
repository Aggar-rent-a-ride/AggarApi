using API.Hubs;
using API.Services;
using CORE.BackgroundJobs;
using CORE.BackgroundJobs.IBackgroundJobs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Auth;
using CORE.DTOs.Chat;
using CORE.DTOs.Email;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Keys;
using CORE.DTOs.Paths;
using CORE.DTOs.Payment;
using CORE.Helpers;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Context;
using DATA.DataAccess.Context.Interceptors;
using DATA.DataAccess.Repositories;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Serilog;
using Serilog.Formatting.Compact;
using Stripe;
using System;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();

            // Configure Serilog for file logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            // Use Serilog as the logging provider
            builder.Host.UseSerilog();

            // Add Hangfire services
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer();

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                }); ;
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            
            /*
            // Register GeometryFactory for spatial operations
            builder.Services.AddSingleton<NtsGeometryServices>(NtsGeometryServices.Instance);
            builder.Services.AddSingleton<GeometryFactory>(provider =>
            {
                NtsGeometryServices geometryServices = provider.GetRequiredService<NtsGeometryServices>();
                return geometryServices.CreateGeometryFactory(srid: 4326);
            });*/

            // register DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SQLServer"),
                sqlOptions => sqlOptions
                .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                )
                .AddInterceptors(new SoftDeleteInterceptor())
            );

            // identity
            builder.Services.AddIdentity<AppUser, IdentityRole<int>>(
            options =>
            {
            }
            ).AddEntityFrameworkStores<AppDbContext>();

            // Configure Swagger to use JWT Bearer token
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AggarAPI", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                    }});
            });


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        // Read the token for SignalR connections
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Chat"))
                        {
                            context.Token = accessToken;
                        }
                        // Read the token for SignalR connections
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Notification"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };

            });

            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<GeoapifyAddressRequest>(builder.Configuration.GetSection("GeoapifyAddressRequest"));
            builder.Services.Configure<Paths>(builder.Configuration.GetSection("Paths"));
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            builder.Services.Configure<WarningManagement>(builder.Configuration.GetSection("WarningManagement"));
            builder.Services.Configure<PaymentPolicy>(builder.Configuration.GetSection("PaymentPolicy"));
            builder.Services.Configure<SecretKeys>(builder.Configuration.GetSection("Secret"));

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IGeoapifyService, GeoapifyService>();
            builder.Services.AddScoped<IFileService, CORE.Services.FileService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IEmailTemplateRendererService, EmailTemplateRendererService>();
            builder.Services.AddScoped<IFileCacheService, FileCacheService>();
            builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
            builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            builder.Services.AddScoped<IReviewService, CORE.Services.ReviewService>();
            builder.Services.AddScoped<IRentalService, RentalService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IRentalReviewService, RentalReviewService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IUserReviewService, UserReviewService>();
            builder.Services.AddScoped<IVehicleReviewService, VehicleReviewService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
            builder.Services.AddScoped<IHashingService, HashingService>();
            builder.Services.AddScoped<IQrCodeService, QrCodeService>();

            builder.Services.AddScoped<IUserRatingUpdateJob, UserRatingUpdateJob>();
            builder.Services.AddScoped<IUserManagementJob, UserManagementJob>();
            builder.Services.AddScoped<IEmailSendingJob, EmailSendingJob>();
            builder.Services.AddScoped<IVehicleRatingUpdateJob, VehicleRatingUpdateJob>();
            builder.Services.AddScoped<IVehiclePopularityManagementJob, VehiclePopularityManagementJob>();
            builder.Services.AddScoped<IBookingReminderJob, BookingReminderJob>();
            builder.Services.AddScoped<INotificationJob, NotificationJob>();
            builder.Services.AddScoped<IBookingHandlerJob, BookingHandlerJob>();
            builder.Services.AddScoped<IRentalHandlerJob, RentalHandlerJob>();

            builder.Services.AddHttpClient<IGeoapifyService, GeoapifyService>();

            builder.Services.AddMemoryCache();
            builder.Services.AddOutputCache();

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 8 * 1024 * 1024;
            });

            //builder.Services.AddRateLimiter(options =>
            //{
            //    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            //    {
            //        var ipAddress = context.Connection.RemoteIpAddress;
            //        return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
            //            _ => new FixedWindowRateLimiterOptions
            //            {
            //                PermitLimit = 50, // Allow 100 requests
            //                Window = TimeSpan.FromMinutes(1), // Per 1 minute window
            //                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            //                QueueLimit = 0
            //            });
            //    });
            //});

            var app = builder.Build();
            
            app.UseOutputCache();

            //app.UseRateLimiter();

            app.MapOpenApi();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            //else
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI(options => {
            //        options.SwaggerEndpoint("/openapi/v1.json", "v1");
            //        options.RoutePrefix = string.Empty;
            //    });
            //}

            app.Use(async (context, next) =>
            {
                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host.Value}/";
                context.Items["BaseUrl"] = baseUrl;
                await next();
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHangfireDashboard();


            app.MapHub<ChatHub>("/Chat");
            app.MapHub<NotificationHub>("/Notification");

            app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseSerilogRequestLogging();

            app.Run();
        }
    }
}
