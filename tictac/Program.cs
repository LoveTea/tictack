
using Microsoft.EntityFrameworkCore;
using tictac.Data;
using tictac.Hubs;
using tictac.Interfaces;
using tictac.Middleware;
using tictac.Services;

namespace tictac
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5173",
                            "https://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IHubService, HubService>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            builder.Services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
            builder.Services.AddHostedService<RoomCleanupService>();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }


            app.UseCors("CorsPolicy");
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<GameHub>("/gamehub");

            await app.RunAsync();
        }
    }
}
