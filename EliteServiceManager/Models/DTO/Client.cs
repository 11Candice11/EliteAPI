using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

[DynamoDBTable("Clients")]
public class Client
{
    [DynamoDBHashKey]
    public string IDNumber { get; set; }

    [DynamoDBProperty]
    public string FirstNames { get; set; }
    
    [DynamoDBProperty]
    public string Surname { get; set; }
    
    [DynamoDBProperty]
    public string RegisteredName { get; set; }
    
    [DynamoDBProperty]
    public string Title { get; set; }
    
    [DynamoDBProperty]
    public string Nickname { get; set; }
    
    [DynamoDBProperty]
    public string AdvisorName { get; set; }
    
    [DynamoDBProperty]
    public string Email { get; set; }
    
    [DynamoDBProperty]
    public string CellPhoneNumber { get; set; }
    
    [DynamoDBProperty]
    public string ConsultantIDNumber { get; set; } // Foreign key linking to Users
}