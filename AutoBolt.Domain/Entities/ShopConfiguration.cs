namespace AutoBolt.Domain.Entities;

public class ShopConfiguration
{
    public int Id { get; set; }
    public string ShopName { get; set; } = "AutoBolt";
    public string Tagline { get; set; } = "Modern Vehicle Service Center";
    public string Address { get; set; } = "Kathmandu, Nepal";
    public decimal LoyaltyThreshold { get; set; } = 5000;
    public decimal LoyaltyDiscountPercent { get; set; } = 10;
}
