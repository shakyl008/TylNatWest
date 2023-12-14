using LondonStockExchange.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace LondonStockExchange.Repositories
{
    public interface IStockRepository
    {
        public Task<Stock> GetStockValueByTickerAsync(string ticker);

        public Task<List<Stock>> GetAllStocksAsync();

        public Task<List<Stock>> GetStocksByTickerListAsync(List<string> listOfTickers);

        public Task AddNewTradeAsync(List<Trade> trades);
    }

    public class StockRepository : IStockRepository
    {
        private readonly StockExchangeDbContext _dbContext;
        private readonly ILogger<StockRepository> _logger;

        public StockRepository(StockExchangeDbContext dbContext, ILogger<StockRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Stock> GetStockValueByTickerAsync(string ticker)
        {
            // input check 
            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Ticker needs to be a valid input");
            }
            try
            {
                // this database call would be wrapped around a Polly policy to be able to implement exponential backoff with jitter
                // this prevents/helps repeat retries from overloading a database

                // if nothing is found this will throw an exception by default
                var stockWithMatchingTicker = await _dbContext.Stocks.Where(stock => stock.Ticker == ticker).FirstAsync();
                return stockWithMatchingTicker;

            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Transient database error occurred when trying to retrieve stock with ticker: {ticker}.", ticker);
                throw new Exception(message: $"There was a transient issue with the database while retrieving stock for ticker '{ticker}'. Please try again later.", innerException: ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Ticker: {ticker} does not exist in the database.", ex);
            }
        }

        public async Task<List<Stock>> GetAllStocksAsync()
        {
            try
            {
                // would check a cache first before making the database call

                // this has the potential to be a very expensive database call
                var allStocks = await _dbContext.Stocks.ToListAsync();

                // this would be cached

                return allStocks;
            }
            catch (DbException ex)
            {
                _logger.LogError("Failed to run GetAllStocksAsync(), see exception: {message}", ex.Message);
                throw new Exception(message: "There was an issue with the database, please again later.", ex);
            }
        }

        public async Task<List<Stock>> GetStocksByTickerListAsync(List<string> listOfTickers)
        {
            // check input 
            if (listOfTickers == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                // query the database to get every stock that matches the ticker in the list of tickers
                var stocksMatchingTicker = await _dbContext.Stocks.Where(stock => listOfTickers.Contains(stock.Ticker)).ToListAsync();

                return stocksMatchingTicker;
            }
            catch (DbException ex)
            {
                _logger.LogError("Failed to run GetStocksByTickerListAsync(), see exception: {message}", ex.Message);
                throw new Exception(message: "There was an issue with the database, please again later.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"List of tickers provided: {listOfTickers} does not exist in the database.", ex);
            }
        }

        /// <summary>
        /// Takes in a list of trades that have occured, processes the list to obtain a list of stocks. 
        /// This list of stocks will contain the most recent value of the stock which will be written to the database.
        /// </summary>
        /// <param name="trades"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task AddNewTradeAsync(List<Trade> trades)
        {
            // input check
            if (trades == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                // write to database
                await _dbContext.Transactions.AddRangeAsync(trades);

                // sort and organise the list of Trades to extract the stocks to be updated
                var orderedAndGroupedTrades = trades.OrderByDescending(trade => trade.TimeOfTrade).GroupBy(trade => trade.StockTicker);

                // trades can contain multiple trade of the same stock, I want the latest value of the stock price
                List<Stock> latestStocks = new List<Stock>();
                foreach (var groupedTrades in orderedAndGroupedTrades)
                {
                    var stock = new Stock()
                    {
                        Ticker = groupedTrades.First().StockTicker,
                        TradePrice = groupedTrades.First().TradePrice,
                    };
                    latestStocks.Add(stock);
                }

                _dbContext.Stocks.UpdateRange(latestStocks);

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // transient errors
                // the exception handlding would be done by Polly in a production scenario
                // for now just throwing the exception up the call stack
                _logger.LogError(ex, "Database related error occured when call AddNewTradesAsync() for {trades}", trades);
                throw new DbUpdateException(message: "And error occured in trying to add to the database, please again later.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("Failed to add new trades with list: {trades}, please see error: {message}", trades, ex.Message);
                throw new InvalidOperationException(message: "", ex);
            }
        }
    }
}
