namespace EliteService.EliteServiceManager.Models.Request;

public class PortfolioRating
{
    public string Key { get; set; }            // Composite key: InstrumentName::IsinNumber
    public string IsinNumber { get; set; }     // Still needed!
    public string InstrumentName { get; set; }
    public string ClientID { get; set; }
    public string LastUpdated { get; set; }
    public string Rating6Months { get; set; }
    public string Rating1Year { get; set; }
    public string Rating3Years { get; set; }

    // private void _uploadExcel() { /* implementation */ }
    // private void selectExcelFile() { /* implementation */ }
    // private void processAllPdfs() { /* implementation */ }
    // private void extractFundFactsData() { /* implementation */ }
    // private void extractReturns() { /* implementation */ }

    // public void updatePortfolioRatings() {
    //     // Updated logic that uses Rating6Months, Rating1Year, and Rating3Years
    // }
}