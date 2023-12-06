using LondonStockExchange.Models;
using LondonStockExchange.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LondonStockExchange.Controllers
{
    [Route("LondonStockExchange/v1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class StockController : ControllerBase
    {

        private readonly IStockService _stockService;
        private readonly ILogger<StockController> _logger;

        public StockController(IStockService stockService, ILogger<StockController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        [HttpGet("GetStockValueByTicker/{ticker}")]
        public async Task<IActionResult> GetStockValueByTickerAsync(string ticker)
        {
            try
            {
                var stock = await _stockService.GetStockValueByTickerAsync(ticker);
                if (stock == null)
                {
                    return NotFound($"Stock with ticker {ticker} not found.");
                }
                else
                {
                    return Ok(stock);
                }
            }
            catch (DbUpdateException ex)
            {
                // error for database
                return StatusCode(500, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // exceptions are allowed to bubble up and caught at controller layer
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetStockValueByTickerAsync for ticker: {ticker}. The exception message raised is {message}", ticker, ex.Message);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetAllStocks")]
        public async Task<IActionResult> GetAllStocksAsync()
        {
            try
            {
                var allStocks = await _stockService.GetAllStocksAsync();
                return Ok(allStocks);
            }
            catch (DbUpdateException ex)
            {
                // exceptions are allowed to bubble up and caught at controller layer
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllStocksAsync. See exception message: {message}", ex.Message);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetStocksByTickerList")]
        public async Task<IActionResult> GetStocksByTickerListAsync([FromBody] List<string> listOfTickers)
        {
            try
            {
                var stocks = await _stockService.GetStocksByTickerListAsync(listOfTickers);
                if(stocks == null || stocks.Count == 0)
                {
                    return NotFound("No stocks corresponding to these tickers");
                }
                else
                {
                    return Ok(stocks);
                }
            }
            catch (DbUpdateException ex)
            {
                // exceptions are allowed to bubble up and caught at controller layer
                return StatusCode(500, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetStocksByTickerListAsync when using {list}", listOfTickers);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("AddNewTrade")]
        public async Task<IActionResult> AddNewTradeAsync([FromBody] List<Trade> trades)
        {
            try
            {
                await _stockService.AddNewTradeAsync(trades);
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                // exceptions are allowed to bubble up and caught at controller layer
                return StatusCode(500, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddNewTradeAsync trying to add trades: {trades}", trades);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
