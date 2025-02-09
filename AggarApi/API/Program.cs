using API.Hubs;
using CORE.DTOs.Auth;
using CORE.DTOs.Email;
using CORE.DTOs.Geoapify;
using CORE.DTOs.Paths;
using CORE.Services;
using CORE.Services.IServices;
using DATA.DataAccess.Context;
using DATA.DataAccess.Context.Interceptors;
using DATA.DataAccess.Repositories;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Security.Claims;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Register GeometryFactory for spatial operations
            builder.Services.AddSingleton<NtsGeometryServices>(NtsGeometryServices.Instance);
            builder.Services.AddSingleton<GeometryFactory>(provider =>
            {
                NtsGeometryServices geometryServices = provider.GetRequiredService<NtsGeometryServices>();
                return geometryServices.CreateGeometryFactory(srid: 4326);
            });

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
                        return Task.CompletedTask;
                    }
                };

            });
            builder.Services.AddAuthorization();

            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<GeoapifyAddressRequest>(builder.Configuration.GetSection("GeoapifyAddressRequest"));
            builder.Services.Configure<Paths>(builder.Configuration.GetSection("Paths"));

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IGeoapifyService, GeoapifyService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IUserConnectionService, UserConnectionService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddHttpClient<IGeoapifyService, GeoapifyService>();
            builder.Services.AddMemoryCache();

            builder.Services.AddSignalR();

            var app = builder.Build();
            app.MapOpenApi();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                    options.RoutePrefix = string.Empty;
                });

            app.Use(async (context, next) =>
            {
                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host.Value}/";
                context.Items["BaseUrl"] = baseUrl;
                await next();
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<ChatHub>("/Chat");

            app.Run();
        }
    }
}
