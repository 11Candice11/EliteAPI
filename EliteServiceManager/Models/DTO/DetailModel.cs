namespace EliteService.EliteServiceManager.Models.DTO;

public class DetailModel
{
    public string? IDNumber { get; set; }
    public string InstrumentName { get; set; }
    public string? ProductDescription { get; set; }
    public string? ReportingName { get; set; }
    public string ReferenceNumber { get; set; }
    public DateTime InceptionDate { get; set; }
    public decimal InitialContributionAmount { get; set; }
    public string InitialContributionCurrencyAbbreviation { get; set; }
    public decimal RegularContributionAmount { get; set; }
    public string RegularContributionCurrencyAbbreviation { get; set; }
    public string RegularContributionFrequency { get; set; }
    public decimal RegularContributionEscalationPercentage { get; set; }
    public decimal RegularWithdrawalAmount { get; set; }
    public decimal RegularWithdrawalPercentage { get; set; }
    public string RegularWithdrawalCurrencyAbbreviation { get; set; }
    public string RegularWithdrawalFrequency { get; set; }
    public decimal RegularWithdrawalEscalationPercentage { get; set; }
    public string ReportNotes { get; set; }
    public List<PortfolioEntryTreeModel> PortfolioEntryTreeModels { get; set; }
    public List<TransactionModel> TransactionModels { get; set; }
    public List<RootValueDateModel> RootValueDateModels { get; set; }

}