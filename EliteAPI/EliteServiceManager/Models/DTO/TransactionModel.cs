namespace EliteService.EliteServiceManager.Models.DTO;

public class TransactionModel
{
    public Guid PortfolioEntryId { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyAbbreviation { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal ConvertedAmount { get; set; }
}