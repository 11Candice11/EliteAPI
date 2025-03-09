using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("ClientData")]
public class ClientData
{
    [DynamoDBHashKey]
    public string ClientID { get; set; } // Links to Client ID
    
    [DynamoDBProperty]
    public List<string> ListDates { get; set; } = new List<string>();
    
    [DynamoDBProperty]
    public string ConsultantIDNumber { get; set; } // Foreign key linking to Users
}