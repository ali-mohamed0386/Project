using JobApplication.API.Middleware;
using JobApplication.API.Services;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Repositories;
using JobApplication.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace JobApplication.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------------------------------------------------------
            // 1) Controllers + JSON
            // ------------------------------------------------------------------
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    // Serializes enums as strings ("Applied") instead of numbers
                    // (0). Very useful for front-end clients and API testing.
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // ------------------------------------------------------------------
            // 2) Database
            // ------------------------------------------------------------------
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ------------------------------------------------------------------
            // 3) JWT settings + validation
            // ------------------------------------------------------------------
            // The secret is never hard-coded — it comes from appsettings,
            // User Secrets, or environment variables. The code only verifies
            // that it is present and valid.
            var jwtOptions = builder.Configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
                ?? throw new InvalidOperationException(
                    $"Configuration section '{JwtOptions.SectionName}' was not found.");

            if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
            {
                // HMAC-SHA256 requires a key of at least 256 bits = 32 bytes.
                // Failing fast is better than running with a weakly signed token.
                throw new InvalidOperationException(
                    "Jwt:Secret is missing or shorter than 32 characters. " +
                    "Set it via appsettings.Development.json, User Secrets, or an environment variable.");
            }

            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.SectionName));

            // ------------------------------------------------------------------
            // 4) Authentication (JWT Bearer) + Authorization
            // ------------------------------------------------------------------
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,
                        // By default this allows a 5-minute clock skew, which
                        // extends the token's validity past its expiry. Setting
                        // it to zero makes expiry strict.
                        ClockSkew = TimeSpan.Zero,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                        // Tells the middleware exactly which claims carry the
                        // user identity and role.
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };

                    // Set explicitly rather than relying on the default, so that
                    // "nameid" in the token maps back to
                    // ClaimTypes.NameIdentifier — which is what
                    // CurrentUserService reads.
                    options.MapInboundClaims = true;
                });

            builder.Services.AddAuthorization();

            // ------------------------------------------------------------------
            // 5) Dependency Injection
            // ------------------------------------------------------------------
            // Services are registered by interface rather than by concrete
            // class, so controllers know nothing about Infrastructure.
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Repositories (Infrastructure)
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
            builder.Services.AddScoped<IRecruiterRepository, RecruiterRepository>();

            // Security (Infrastructure)
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddSingleton<ITokenService, JwtTokenService>();

            // MediatR (CQRS)
            // Scans the Application assembly for every IRequestHandler and
            // registers it, so each new command handler is picked up without
            // touching this file. Controllers depend only on IMediator, which
            // keeps them free of any reference to the Application features.
            builder.Services.AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(typeof(CloseJobCommand).Assembly));

            // ------------------------------------------------------------------
            // 6) OpenAPI / Scalar
            // ------------------------------------------------------------------
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // ------------------------------------------------------------------
            // 7) HTTP request pipeline
            // ------------------------------------------------------------------

            // The exception middleware must be first so that it catches
            // exceptions thrown by any middleware or controller after it.
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            // WARNING: order matters here.
            // UseAuthentication runs first, reading the token and building the
            // user identity; UseAuthorization then decides whether that identity
            // is permitted. Reversing them makes every [Authorize] return 401,
            // even with a valid token.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
