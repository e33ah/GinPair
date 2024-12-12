using GinPair.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using EFCore.NamingConventions.Internal;

namespace GinPair
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<GinPairDbContext>(
                options => {
                    options.UseNpgsql(connectionString: connectionString)
                    .UseSnakeCaseNamingConvention();
                    }
                ); 


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
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
