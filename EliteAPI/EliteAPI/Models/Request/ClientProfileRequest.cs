namespace EliteAPI.Models.Request;
public class ClientProfileRequest
{
    public string TransactionDateStart { get; set; }
    public string TransactionDateEnd { get; set; }
    public int TargetCurrencyL { get; set; }
    public List<string> ValueDates { get; set; }
    public List<InputEntityModel> InputEntityModels { get; set; }
}