using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chat;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.DefaultChores;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Notifications.Email;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Features.Notifications.Push;
using ChoreBuddies.Backend.Features.PredefinedRewards;
using ChoreBuddies.Backend.Features.RedeemedRewards;
using ChoreBuddies.Backend.Features.RedeemRewards;
using ChoreBuddies.Backend.Features.Reminders;
using ChoreBuddies.Backend.Features.Rewards;
using ChoreBuddies.Backend.Features.ScheduledChores;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.SqlServer;
using Maileroo.DotNet.SDK;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ChoreBuddies.Backend;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.AddServiceDefaults();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddEndpointsApiExplorer();

        // Database configuration
        builder.Services.AddDbContext<ChoreBuddiesDbContext>(opt =>
        {
            opt.UseSqlServer(connectionString);
        });

        // Hangfire
        builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170).UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings().UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

        builder.Services.AddHangfireServer();

        // Asp.net Identity
        builder.Services.AddIdentity<AppUser, IdentityRole<int>>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequiredLength = 8;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ChoreBuddiesDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddScoped<ITokenService, TokenService>();

        // Configure JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKeyRaw = jwtSettings["SecretKey"];
        if (secretKeyRaw is null)
            throw new InvalidConfigurationException("Secret key not found");
        var secretKey = Encoding.UTF8.GetBytes(secretKeyRaw);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // For SignalR authentication
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and your token JWT.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5...\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
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
                }
            });
        });
        builder.Services.AddControllers();

        // Chores
        builder.Services.AddScoped<IChoresRepository, ChoresRepository>();
        builder.Services.AddScoped<IChoresService, ChoresService>();
        // Default Chores
        builder.Services.AddScoped<IPredefinedChoreRepository, PredefinedChoreRepository>();
        builder.Services.AddScoped<IPredefinedChoreService, PredefinedChoreService>();
        // ScheduledChore
        builder.Services.AddScoped<IScheduledChoresRepository, ScheduledChoresRepository>();
        builder.Services.AddScoped<IScheduledChoresService, ScheduledChoresService>();
        builder.Services.AddHostedService<ScheduledChoresBackgroundService>();
        // Household
        builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        builder.Services.AddScoped<IHouseholdService, HouseholdService>();
        builder.Services.AddScoped<IInvitationCodeService, InvitationCodeService>();
        // Users
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
        builder.Services.AddScoped<IAppUserService, AppUserService>();
        // Rewards
        builder.Services.AddScoped<IRewardsRepository, RewardsRepository>();
        builder.Services.AddScoped<IRewardsService, RewardsService>();
        // Default Rewards
        builder.Services.AddScoped<IPredefinedRewardsRepository, PredefinedRewardsRepository>();
        builder.Services.AddScoped<IPredefinedRewardsService, PredefinedRewardsService>();
        // Redeemed Rewards
        builder.Services.AddScoped<IRedeemedRewardsRepository, RedeemedRewardsRepository>();
        builder.Services.AddScoped<IRedeemedRewardsService, RedeemedRewardsService>();

        builder.Services.AddSingleton(sp =>
        {
            var apiKey = builder.Configuration["MtaApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "MailerooClient cannot be created: 'MtaApiKey:ApiKey' is missing or empty in configuration."
                );
            }
            return new MailerooClient(apiKey);
        });

        builder.Services.Configure<EmailServiceOptions>(builder.Configuration.GetSection("Maileroo"));

        builder.Services.AddScoped<EmailService>(sp =>
        {
            var client = sp.GetRequiredService<MailerooClient>();
            var options = sp.GetRequiredService<IOptions<EmailServiceOptions>>().Value;

            return new EmailService(
                client,
                options.From,
                options.FromName
            );
        });

        builder.Services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        builder.Services.AddScoped<IEmailService>(sp => sp.GetRequiredService<EmailService>());
        builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        builder.Services.AddScoped<INotificationChannel>(sp => sp.GetRequiredService<EmailService>());
        builder.Services.AddScoped<INotificationChannel, FirebaseNotificationsService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IRemindersService, RemindersService>();
        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        if (FirebaseApp.DefaultInstance == null && Path.Exists(ProgramConstants.FireBaseCredentialsPath))
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(ProgramConstants.FireBaseCredentialsPath)
            });
        }

        var app = builder.Build();

        // Hangfire

        app.UseHangfireDashboard();

        // Apply migrations to database
        await using (var serviceScope = app.Services.CreateAsyncScope())
        await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<ChoreBuddiesDbContext>())
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }
        }

        app.MapDefaultEndpoints();

        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapHub<ChatHub>("/chatHub");

        app.Run();
    }
}
