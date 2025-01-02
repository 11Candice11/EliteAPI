namespace EliteAPI.Models;

public class EntityModel
{
    public string FirstNames { get; set; }
    public string Surname { get; set; }
    public string RegisteredName { get; set; }
    public string Title { get; set; }
    public string Nickname { get; set; }
    public string AdvisorName { get; set; }
    public string Email { get; set; }
    public string CellPhoneNumber { get; set; }
    public List<DetailModel> DetailModels { get; set; }
}