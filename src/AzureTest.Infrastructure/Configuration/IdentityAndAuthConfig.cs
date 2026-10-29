using System;
using System.Text;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AzureTest.Infrastructure.Configuration;

public static class IdentityAndAuthConfig
{
    public static IServiceCollection AddIdentityAndAuthentication(
        this IServiceCollection services,
        InfrastructureFeatureOptions featureOptions,
        IConfiguration configuration)
    {
        // ── 1. Identity Options & Stores ──────────────────────────────────────────
        var identitySettings = new IdentitySettings();
        configuration.GetSection(IdentitySettings.SectionName).Bind(identitySettings);

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = identitySettings.Password.RequireDigit;
            options.Password.RequiredLength = identitySettings.Password.RequiredLength;
            options.Password.RequireNonAlphanumeric = identitySettings.Password.RequireNonAlphanumeric;
            options.Password.RequireUppercase = identitySettings.Password.RequireUppercase;
            options.Password.RequireLowercase = identitySettings.Password.RequireLowercase;
            options.Password.RequiredUniqueChars = identitySettings.Password.RequiredUniqueChars;

            options.Lockout.AllowedForNewUsers = identitySettings.Lockout.AllowedForNewUsers;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.Lockout.DefaultLockoutTimeSpanMinutes);
            options.Lockout.MaxFailedAccessAttempts = identitySettings.Lockout.MaxFailedAccessAttempts;

            options.User.AllowedUserNameCharacters = identitySettings.User.AllowedUserNameCharacters;
            options.User.RequireUniqueEmail = identitySettings.User.RequireUniqueEmail;

            options.SignIn.RequireConfirmedEmail = identitySettings.SignIn.RequireConfirmedEmail;
            options.SignIn.RequireConfirmedPhoneNumber = identitySettings.SignIn.RequireConfirmedPhoneNumber;
        })
        .AddEntityFrameworkStores<AzureTest.Persistence.Context.AzureTestDbContext>()
        .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

        services.AddTransient<IEmailSender<ApplicationUser>, IdentityEmailSender<ApplicationUser>>();

        // ── 2. Email Delivery Configuration ───────────────────────────────────────
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailSenderService, GmailSender>();

        // ── 3. Authentication Schemes (JWT vs Cookie) ─────────────────────────────
        if (featureOptions.UseJWT)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "fallback-super-secret-key-1234567890"))
                };

                // Support extracting token from cookie for MVC views
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("jwt"))
                        {
                            context.Token = context.Request.Cookies["jwt"];
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Auth/Login");
                        return Task.CompletedTask;
                    }
                };
            });
        }
        else
        {
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = featureOptions.EnableLogoutOtherDevices ? TimeSpan.Zero : TimeSpan.FromHours(12);
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".AzureTest.Auth";
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            });
        }

        // ── 4. External Authentication Providers ──────────────────────────────────
        if (featureOptions.UseGoogleAuthenticationOpenIDConnect)
        {
            services.AddAuthentication().AddGoogleOpenIdConnect(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });
        }

        if (featureOptions.UseGoogleOAuthAuthentication)
        {
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });
        }

        return services;
    }
}
