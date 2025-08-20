namespace ChoreBuddies.Backend;

public class Program
{
    public static void Main(string[] args)
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
        //builder.Services.AddMvc();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        // Add services to the container.
        builder.Services.AddRazorPages();


        var app = builder.Build();

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
