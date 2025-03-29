using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EliteService.EliteServiceManager.Models;
using EliteService.EliteServiceManager.Models.DTO;
using EliteService.EliteServiceManager.Models.Response;

namespace EliteService.EliteServiceManager.Services
{
    public class ClientService
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public ClientService(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<EntityModel> GetClientInfo(string idNumber)
        {
            try
            {
                // ✅ Step 1: Fetch Client Basic Info from `EntityModel` Table
                var entityRequest = new GetItemRequest
                {
                    TableName = "EntityModel",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "IDNumber", new AttributeValue { S = idNumber } }
                    }
                };

                var entityResponse = await _dynamoDbClient.GetItemAsync(entityRequest);

                if (entityResponse.Item == null || entityResponse.Item.Count == 0)
                {
                    Console.WriteLine("❌ Client not found in EntityModel table.");
                    return null;
                }

                var entity = new EntityModel
                {
                    FirstNames = entityResponse.Item.TryGetValue("FirstNames", out var firstNameAttr)
                        ? firstNameAttr.S
                        : "N/A",
                    Surname = entityResponse.Item.TryGetValue("Surname", out var surnameAttr) ? surnameAttr.S : "N/A",
                    RegisteredName = entityResponse.Item.TryGetValue("RegisteredName", out var registeredNameAttr)
                        ? registeredNameAttr.S
                        : "N/A",
                    Title = entityResponse.Item.TryGetValue("Title", out var titleAttr) ? titleAttr.S : "N/A",
                    Nickname = entityResponse.Item.TryGetValue("Nickname", out var nicknameAttr)
                        ? nicknameAttr.S
                        : "N/A",
                    AdvisorName = entityResponse.Item.TryGetValue("AdvisorName", out var advisorAttr)
                        ? advisorAttr.S
                        : "N/A",
                    Email = entityResponse.Item.TryGetValue("Email", out var emailAttr) ? emailAttr.S : "N/A",
                    IDNumber = entityResponse.Item.TryGetValue("Email", out var idNumberAttr) ? idNumberAttr.S : "N/A",
                    ConsultantIDNumber = entityResponse.Item.TryGetValue("Email", out var consultantIdNumberAttr)
                        ? consultantIdNumberAttr.S
                        : "N/A",
                    CellPhoneNumber = entityResponse.Item.TryGetValue("CellPhoneNumber", out var phoneAttr)
                        ? phoneAttr.S
                        : "N/A"
                };

                // ✅ Step 2: Fetch Investment Plans from `DetailModel` Table
                entity.DetailModels = await GetDetailModelsByClient(idNumber);

                Console.WriteLine(
                    $"✅ Full Client Data Retrieved: {Newtonsoft.Json.JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented)}");

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving client data: {ex.Message}");
                return null;
            }
        }

        private async Task<List<DetailModel>> GetDetailModelsByClient(string registeredName)
        {
            try
            {
                var scanRequest = new ScanRequest
                {
                    TableName = "DetailModel",
                    FilterExpression = "RegisteredName = :registeredName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":registeredName", new AttributeValue { S = registeredName } }
                    }
                };

                var response = await _dynamoDbClient.ScanAsync(scanRequest);

                if (response.Items.Count == 0)
                {
                    Console.WriteLine("❌ No DetailModels found for client.");
                    return new List<DetailModel>();
                }

                var details = response.Items.Select(map => new DetailModel
                {
                    ReferenceNumber = map.TryGetValue("ReferenceNumber", out var refAttr) ? refAttr.S : "N/A",
                    InstrumentName = map.TryGetValue("InstrumentName", out var instAttr) ? instAttr.S : "N/A",
                    ProductDescription = map.TryGetValue("ProductDescription", out var prodAttr) ? prodAttr.S : "N/A",
                    ReportingName = map.TryGetValue("ReportingName", out var repAttr) ? repAttr.S : "N/A",
                    InceptionDate = map.TryGetValue("InceptionDate", out var incAttr)
                        ? DateTime.Parse(incAttr.S)
                        : DateTime.MinValue,
                    InitialContributionAmount = map.TryGetValue("InitialContributionAmount", out var amountAttr)
                        ? decimal.Parse(amountAttr.N)
                        : 0,
                    InitialContributionCurrencyAbbreviation =
                        map.TryGetValue("InitialContributionCurrencyAbbreviation", out var currencyAttr)
                            ? currencyAttr.S
                            : "N/A"
                }).ToList();

                // ✅ Fetch PortfolioEntries for each DetailModel
                foreach (var detail in details)
                {
                    detail.PortfolioEntryTreeModels = await GetPortfolioEntriesByReference(detail.ReferenceNumber);
                }

                return details;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving DetailModels: {ex.Message}");
                return new List<DetailModel>();
            }
        }

        private async Task<List<PortfolioEntryTreeModel>> GetPortfolioEntriesByReference(string referenceNumber)
        {
            try
            {
                var request = new QueryRequest
                {
                    TableName = "PortfolioEntryTreeModel",
                    KeyConditionExpression = "ReferenceNumber = :ref",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":ref", new AttributeValue { S = referenceNumber } }
                    }
                };

                var response = await _dynamoDbClient.QueryAsync(request);

                if (response.Items.Count == 0)
                {
                    return new List<PortfolioEntryTreeModel>();
                }

                var portfolios = response.Items.Select(map => new PortfolioEntryTreeModel
                {
                    PortfolioEntryId = map.TryGetValue("PortfolioEntryId", out var idAttr)
                        ? Guid.Parse(idAttr.S)
                        : Guid.NewGuid(),
                    InstrumentName = map.TryGetValue("InstrumentName", out var instAttr) ? instAttr.S : "N/A",
                    IsinNumber = map.TryGetValue("IsinNumber", out var isinAttr) ? isinAttr.S : "N/A"
                }).ToList();

                return portfolios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving PortfolioEntries: {ex.Message}");
                return new List<PortfolioEntryTreeModel>();
            }
        }

        public async Task<bool> SaveClient(ClientProfileResponse clientData)
        {
            try
            {
                var tasks = new List<Task>();

                foreach (var entity in clientData.EntityModels)
                {
                    tasks.Add(SaveEntityModel(entity));

                    foreach (var detail in entity.DetailModels)
                    {
                        tasks.Add(SaveDetailModel(detail));

                        foreach (var portfolio in detail.PortfolioEntryTreeModels)
                        {
                            tasks.Add(SavePortfolioEntry(portfolio));
                        }

                        foreach (var transaction in detail.TransactionModels)
                        {
                            tasks.Add(SaveTransaction(transaction));
                        }

                        foreach (var rootValue in detail.RootValueDateModels)
                        {
                            tasks.Add(SaveRootValueDate(rootValue));
                        }
                    }
                }

                await Task.WhenAll(tasks); // ✅ Run all tasks in parallel

                Console.WriteLine("✅ All client data saved successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving client data: {ex.Message}");
                return false;
            }
        }

        private async Task SaveEntityModel(EntityModel entity)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "FirstNames", new AttributeValue { S = SafeString(entity.FirstNames) } },
                { "Surname", new AttributeValue { S = SafeString(entity.Surname) } },
                { "RegisteredName", new AttributeValue { S = SafeString(entity.RegisteredName) } },
                { "Title", new AttributeValue { S = SafeString(entity.Title) } },
                { "Nickname", new AttributeValue { S = SafeString(entity.Nickname) } },
                { "AdvisorName", new AttributeValue { S = SafeString(entity.AdvisorName) } },
                { "Email", new AttributeValue { S = SafeString(entity.Email) } },
                { "CellPhoneNumber", new AttributeValue { S = SafeString(entity.CellPhoneNumber) } },
                { "IDNumber", new AttributeValue { S = SafeString(entity.IDNumber) } },
                { "ConsultantIDNumber", new AttributeValue { S = SafeString(entity.ConsultantIDNumber) } }
            };

            var request = new PutItemRequest
            {
                TableName = "EntityModel",
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        private async Task SaveDetailModel(DetailModel detail)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "ReferenceNumber", new AttributeValue { S = SafeString(detail.ReferenceNumber) } },
                { "InstrumentName", new AttributeValue { S = SafeString(detail.InstrumentName) } },
                { "ProductDescription", new AttributeValue { S = SafeString(detail.ProductDescription) } },
                { "ReportingName", new AttributeValue { S = SafeString(detail.ReportingName) } },
                { "InceptionDate", new AttributeValue { S = detail.InceptionDate.ToString("o") } },
                { "InitialContributionAmount", new AttributeValue { N = detail.InitialContributionAmount.ToString() } },
                {
                    "InitialContributionCurrencyAbbreviation",
                    new AttributeValue { S = SafeString(detail.InitialContributionCurrencyAbbreviation) }
                }
            };

            var request = new PutItemRequest
            {
                TableName = "DetailModel",
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        private async Task SavePortfolioEntry(PortfolioEntryTreeModel portfolio)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "PortfolioEntryId", new AttributeValue { S = portfolio.PortfolioEntryId.ToString() } },
                { "InstrumentName", new AttributeValue { S = SafeString(portfolio.InstrumentName) } },
                { "IsinNumber", new AttributeValue { S = SafeString(portfolio.IsinNumber) } },
                { "MorningStarId", new AttributeValue { S = SafeString(portfolio.MorningStarId) } }
            };

            var request = new PutItemRequest
            {
                TableName = "PortfolioEntryTreeModel",
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        private async Task SaveTransaction(TransactionModel transaction)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "TransactionId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                { "PortfolioEntryId", new AttributeValue { S = transaction.PortfolioEntryId.ToString() } },
                { "TransactionType", new AttributeValue { S = SafeString(transaction.TransactionType) } },
                { "TransactionDate", new AttributeValue { S = transaction.TransactionDate.ToString("o") } },
                { "CurrencyAbbreviation", new AttributeValue { S = SafeString(transaction.CurrencyAbbreviation) } },
                { "ExchangeRate", new AttributeValue { N = transaction.ExchangeRate.ToString() } },
                { "ConvertedAmount", new AttributeValue { N = transaction.ConvertedAmount.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = "TransactionModel",
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        private async Task SaveRootValueDate(RootValueDateModel rootValue)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "RootValueId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                { "RootPortfolioEntryId", new AttributeValue { S = rootValue.RootPortfolioEntryId.ToString() } },
                { "ValueType", new AttributeValue { S = SafeString(rootValue.ValueType) } },
                { "ConvertedValueDate", new AttributeValue { S = rootValue.ConvertedValueDate.ToString("o") } },
                { "CurrencyAbbreviation", new AttributeValue { S = SafeString(rootValue.CurrencyAbbreviation) } },
                { "TotalConvertedAmount", new AttributeValue { N = rootValue.TotalConvertedAmount.ToString() } }
            };

            var request = new PutItemRequest
            {
                TableName = "RootValueDateModel",
                Item = item
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        // ✅ Utility method to replace null/empty values with "N/A"
        private static string SafeString(string value)
        {
            return string.IsNullOrEmpty(value) ? "N/A" : value;
        }
    }
}