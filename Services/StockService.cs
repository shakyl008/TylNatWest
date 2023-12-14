using LondonStockExchange.Models;
using LondonStockExchange.Repositories;
using Microsoft.EntityFrameworkCore;
using static LondonStockExchange.Controllers.StockController;

namespace LondonStockExchange.Services
{
    public interface IStockService
    {
        public Task<Stock> GetStockValueByTickerAsync(string ticker);

        public Task<List<Stock>> GetAllStocksAsync();

        public Task<List<Stock>> GetStocksByTickerListAsync(List<string> listOfTickers);

        public Task AddNewTradeAsync(List<NewTradeDTO> trades);
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

        public async Task AddNewTradeAsync(List<NewTradeDTO> trades)
        {
            // input check
            if (trades == null)
            {
                throw new ArgumentNullException();
            }

            // convert trade dto to trades
            var listOfTrades = new List<Trade>();


            await _stockRepository.AddNewTradeAsync(listOfTrades);
        }

        internal async Task<List<Trade>> Convert_NewTradeDTOs_To_ListOfTrade(List<NewTradeDTO> newTradeDTO_List)
        {
            List<Trade> newTrades = new List<Trade>();

            Trade TradeDTO_to_Trade(NewTradeDTO newTradeDTO)
            {
                Trade newTrade = new Trade()
                {
                    TradeId = newTradeDTO.TradeId,
                    NumberOfShares = newTradeDTO.NumberOfShares,
                    BrokerId = newTradeDTO.BrokerId,
                    StockTicker = newTradeDTO.StockTicker,
                    TradePrice = newTradeDTO.TradePrice,
                    TimeOfTrade = newTradeDTO.TimeOfTrade,
                };
                return newTrade;
            }

            if(newTradeDTO_List.Count > 1000)
            {
                Parallel.ForEach(newTradeDTO_List, newTradeDTO =>
                {
                    newTrades.Add(TradeDTO_to_Trade(newTradeDTO));
                });
            }
            else
            {
                foreach (var tradeDTO in newTradeDTO_List)
                {
                    newTrades.Add(TradeDTO_to_Trade(tradeDTO));
                }
            }

            return newTrades;
        }
    }
}
