using System.ComponentModel.DataAnnotations;

namespace LondonStockExchange.Models
{
    public class Stock
    {
        /// <summary>
        /// It is assumed that the database, in its simple form, will only contain one entry per Ticker.
        /// Any changes to an existing stock will update this value
        /// </summary>
        [Key]
        [MinLength(1, ErrorMessage = "Ticker needs to be at least 1 charcter longer.")]
        public string Ticker { get; set; }

        [Required(ErrorMessage = "A price is required.")]
        public decimal TradePrice { get; set; }
    }
}
