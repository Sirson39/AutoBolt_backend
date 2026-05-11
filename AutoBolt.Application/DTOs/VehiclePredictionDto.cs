namespace AutoBolt.Application.DTOs;

public class VehiclePredictionDto
{
    public int VehicleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> Predictions { get; set; } = new();
    public DateTime AnalysedAt { get; set; }
}
