namespace EliteService.EliteServiceManager.Models.DTO;


public class ValueModel
{
    public Guid PortfolioEntryId { get; set; }
    public DateTime ValueDate { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal PortfolioSharePercentage { get; set; }
}