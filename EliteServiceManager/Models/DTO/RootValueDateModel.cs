namespace EliteService.EliteServiceManager.Models.DTO;

public class RootValueDateModel
{
    public Guid RootPortfolioEntryId { get; set; } // Root Portfolio Entry ID
    public string ValueType { get; set; } // Value Type (e.g., Market Value)
    public DateTime ConvertedValueDate { get; set; } // Date of the Market Value
    public string CurrencyAbbreviation { get; set; } // Currency
    public decimal TotalConvertedAmount { get; set; } // Total Market Value
    public List<ValueModel> ValueModels { get; set; } // Breakdown of Values
}