using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SemesterTwo.Controllers;
using SemesterTwo.Services;

namespace ST10355869_CLDV6212_Part1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            // Register custom services
            builder.Services.AddSingleton<BlobService>();
            builder.Services.AddSingleton<TableService>();
            builder.Services.AddSingleton<QueueService>();
            builder.Services.AddSingleton<FileService>();

            builder.Services.AddHttpClient<HomeController>(client =>
            {
                client.BaseAddress = new Uri("https://st10355869FunctionApp.azurewebsites.net");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}