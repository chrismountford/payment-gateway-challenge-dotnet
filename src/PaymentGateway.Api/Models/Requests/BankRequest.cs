public class BankRequest
{
    public long card_number { get; set; }
    public string expiry_date { get; set; }
    public string currency { get; set; }
    public int amount { get; set; }
    public int cvv { get; set; }
}