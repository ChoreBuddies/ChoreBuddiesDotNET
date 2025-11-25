using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chat;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.DefaultChores;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Notifications.Email;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Data;
using Maileroo.DotNet.SDK;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChoreBuddies.Backend;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

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

        //builder.Services.AddMvc();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers();

        // Add services to the container.
        builder.Services.AddRazorPages();
        // Chores
        builder.Services.AddScoped<IChoresRepository, ChoresRepository>();
        builder.Services.AddScoped<IChoresService, ChoresService>();
        // Default chores
        builder.Services.AddScoped<IDefaultChoreRepository, DefaultChoreRepository>();
        builder.Services.AddScoped<IDefaultChoreService, DefaultChoreService>();
        // Household
        builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        builder.Services.AddScoped<IHouseholdService, HouseholdService>();
        builder.Services.AddScoped<IInvitationCodeService, InvitationCodeService>();
        //Users
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
        builder.Services.AddScoped<IAppUserService, AppUserService>();

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

        // Interfaces mapped to the same EmailService instance
        builder.Services.AddScoped<IEmailService>(sp => sp.GetRequiredService<EmailService>());
        builder.Services.AddScoped<INotificationService>(sp => sp.GetRequiredService<EmailService>());

        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        var app = builder.Build();

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

        app.MapRazorPages();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapHub<ChatHub>("/chatHub");

        app.Run();
    }
}
