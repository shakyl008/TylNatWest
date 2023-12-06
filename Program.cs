using LondonStockExchange.Models;
using LondonStockExchange.Repositories;
using LondonStockExchange.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace LondonStockExchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // add dbcontext and an in memory db for prototyping purposes
            builder.Services.AddDbContext<StockExchangeDbContext>(options =>
            options.UseInMemoryDatabase("test"));

            // adding dependencies
            builder.Services.AddScoped<IStockRepository, StockRepository>();
            builder.Services.AddScoped<IStockService, StockService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // ensuring that the test database has the seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<StockExchangeDbContext>();
                // Ensure the database is created
                context.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}