using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LondonStockExchange.Models
{
    public class Trade
    {
        [Key]
        [Required(ErrorMessage = "TradeId is required.")]
        public string TradeId { get; set; }

        [Required(ErrorMessage = "NumberOfShares is required.")]
        public decimal NumberOfShares { get; set; }

        [Required(ErrorMessage = "BrokerId is required")]
        public string BrokerId { get; set; }

        [Required(ErrorMessage = "StockTicker is required")]
        public string StockTicker { get; set; }

        [Required(ErrorMessage = "TradePrice is required")]
        public decimal TradePrice { get; set; }

        [Required(ErrorMessage = "TimeOfTrade is required")]
        public DateTime TimeOfTrade { get; set; }

        // navigation property for lazy loading
        [ForeignKey(nameof(StockTicker))]
        public virtual Stock _Stock { get; set; }
    }
}
