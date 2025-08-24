using ChoreBuddies.Backend.Chore;
using ChoreBuddies.Backend.Tasks;
using ChoreBuddies.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace ChoreBuddies.Backend;

public class Program
{
    public async static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddDbContext<ChoreBuddiesDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        //builder.Services.AddMvc();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers();

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<IChoresService, ChoresService>();

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

        app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );

        app.UseAuthorization();

        app.MapRazorPages();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }
}
