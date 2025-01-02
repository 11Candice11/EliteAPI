namespace EliteAPI.Models;
public class PortfolioEntryTreeModel
{
    public Guid PortfolioEntryId { get; set; }
    public Guid ParentPortfolioEntryId { get; set; }
    public Guid RootPortfolioEntryId { get; set; }
    public int Level { get; set; }
    public bool IsRoot { get; set; }
    public bool IsLowestLevel { get; set; }
    public string InstrumentName { get; set; }
    public string IsinNumber { get; set; }
    public string MorningStarId { get; set; }

}