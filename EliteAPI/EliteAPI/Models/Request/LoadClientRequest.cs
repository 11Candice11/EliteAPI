namespace EliteService.EliteServiceManager.Models.Request;

public class LoadClientRequest
{
    public bool IncludeDeletedEntities { get; set; }
    public bool IncludeOnlyClientsAndPotentialClients { get; set; }
    public bool IncludePortfolioRelations { get; set; }
    public bool IncludeStaffMembers { get; set; }
    public string SearchText { get; set; }
}