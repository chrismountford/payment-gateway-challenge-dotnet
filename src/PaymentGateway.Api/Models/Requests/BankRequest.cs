using System.Text.Json.Serialization;

public class BankRequest
{
    public string card_number { get; set; }
    public string expiry_date { get; set; }
    public string currency { get; set; }
    public string amount { get; set; }
    public string cvv { get; set; }
}