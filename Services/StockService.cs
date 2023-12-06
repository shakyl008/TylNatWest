using LondonStockExchange.Models;
using LondonStockExchange.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LondonStockExchange.Services
{
    public interface IStockService
    {
        public Task<Stock> GetStockValueByTickerAsync(string ticker);

        public Task<List<Stock>> GetAllStocksAsync();

        public Task<List<Stock>> GetStocksByTickerListAsync(List<string> listOfTickers);

        public Task AddNewTradeAsync(List<Trade> trades);
    }
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<StockService> _logger;

        public StockService(IStockRepository stockRepository, ILogger<StockService> logger)
        {
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<Stock> GetStockValueByTickerAsync(string ticker)
        {
            // input check 
            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Ticker needs to be a valid input");
            }
            var stock = await _stockRepository.GetStockValueByTickerAsync(ticker);
            return stock;

            // allowing exceptions to bubble up to controller 
            // as there is no business logic happening at this layer and no expected business related exception
        }

        public async Task<List<Stock>> GetAllStocksAsync()
        {
            var allStocks = await _stockRepository.GetAllStocksAsync();
            return allStocks;
        }

        public async Task<List<Stock>> GetStocksByTickerListAsync(List<string> listOfTickers)
        {
            // check input 
            if (listOfTickers == null)
            {
                throw new ArgumentNullException("Need a valid list of tickers to perform a database search");
            }
            var stocksFilteredByTicker = await _stockRepository.GetStocksByTickerListAsync(listOfTickers);
            return stocksFilteredByTicker;
        }

        public async Task AddNewTradeAsync(List<Trade> trades)
        {
            // input check
            if (trades == null)
            {
                throw new ArgumentNullException();
            }
            await _stockRepository.AddNewTradeAsync(trades);
        }
    }
}
