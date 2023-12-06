using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LondonStockExchange.Models
{
    public class Trade
    {
        [Key]
        [MinLength(1)]
        public string TradeId { get; set; }

        [Required]
        public decimal NumberOfShares { get; set; }

        [Required]
        [MinLength(1)]
        public string BrokerId { get; set; }

        [Required]
        [MinLength(1)]
        public string StockTicker { get; set; }

        [Required]
        public decimal TradePrice { get; set; }

        [Required]
        public DateTime TimeOfTrade { get; set; }

        // navigation property for lazy loading
        [ForeignKey(nameof(StockTicker))]
        public virtual Stock _Stock { get; set; }
    }
}
