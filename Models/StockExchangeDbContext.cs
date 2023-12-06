using Microsoft.EntityFrameworkCore;

namespace LondonStockExchange.Models
{
    public class StockExchangeDbContext : DbContext
    {
        public StockExchangeDbContext(DbContextOptions<StockExchangeDbContext> options) : base(options) { }

        public virtual DbSet<Trade> Transactions { get; set; }

        public virtual DbSet<Stock> Stocks { get; set; }

        // some simple seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Stocks
            modelBuilder.Entity<Stock>().HasData(
                new Stock { Ticker = "A", TradePrice = 150.10m },
                new Stock { Ticker = "B", TradePrice = 2720.57m },
                new Stock { Ticker = "C", TradePrice = 280.30m },
                new Stock { Ticker = "D", TradePrice = 3300.50m },
                new Stock { Ticker = "E", TradePrice = 710.80m },
                new Stock { Ticker = "F", TradePrice = 265.00m },
                new Stock { Ticker = "G", TradePrice = 520.25m },
                new Stock { Ticker = "H", TradePrice = 418000.00m }
            );

            // Seed Trades
            modelBuilder.Entity<Trade>().HasData(
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 100, BrokerId = "BRK01", StockTicker = "A", TimeOfTrade = DateTime.UtcNow.AddDays(-1) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 50, BrokerId = "BRK01", StockTicker = "B", TimeOfTrade = DateTime.UtcNow.AddHours(-2) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 150, BrokerId = "BRK02", StockTicker = "C", TimeOfTrade = DateTime.UtcNow.AddDays(-2) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 10, BrokerId = "BRK03", StockTicker = "D", TimeOfTrade = DateTime.UtcNow.AddHours(-1) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 200, BrokerId = "BRK04", StockTicker = "E", TimeOfTrade = DateTime.UtcNow.AddDays(-3) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 500, BrokerId = "BRK05", StockTicker = "F", TimeOfTrade = DateTime.UtcNow.AddMinutes(-30) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 75, BrokerId = "BRK06", StockTicker = "G", TimeOfTrade = DateTime.UtcNow.AddDays(-4) },
                new Trade { TradeId = Guid.NewGuid().ToString(), NumberOfShares = 1, BrokerId = "BRK07", StockTicker = "H", TimeOfTrade = DateTime.UtcNow.AddDays(-5) }
            );

            base.OnModelCreating(modelBuilder);
        }
    }


}
