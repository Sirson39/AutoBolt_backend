using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class VehiclePredictionService(AutoBoltDbContext context) : IVehiclePredictionService
{
    public async Task<VehiclePredictionDto> AnalyseVehicleAsync(int vehicleId)
    {
        var vehicle = await context.Vehicles.FindAsync(vehicleId)
            ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found.");

        var vehicleInvoices = await context.Invoices
            .Where(i => i.VehicleId == vehicleId)
            .Include(i => i.Items)
            .ThenInclude(ii => ii.Part)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var partNames = vehicleInvoices
            .SelectMany(i => i.Items)
            .Select(ii => ii.Part?.Name?.ToLower() ?? string.Empty)
            .Where(n => n.Length > 0)
            .ToHashSet();

        DateTime? lastServiceDate = vehicleInvoices.FirstOrDefault()?.InvoiceDate;
        int currentYear = DateTime.UtcNow.Year;
        int vehicleAge = currentYear - vehicle.Year;
        double mileage = vehicle.Mileage;

        int score = 0;
        var predictions = new List<string>();

        if (mileage > 80000) { score += 30; }
        else if (mileage >= 50000) { score += 15; }

        if (vehicleAge > 10) { score += 25; }
        else if (vehicleAge >= 7) { score += 12; }

        if (lastServiceDate == null)
        {
            score += 20;
            predictions.Add("No service history found — first service is overdue.");
        }
        else
        {
            var daysSince = (DateTime.UtcNow - lastServiceDate.Value).TotalDays;
            if (daysSince > 180) { score += 25; predictions.Add("Vehicle has not been serviced in over 6 months."); }
            else if (daysSince > 90) { score += 10; predictions.Add("Vehicle approaching the 3-month service interval."); }
        }

        if (!partNames.Any(n => n.Contains("oil") && n.Contains("filter")))
        {
            score += 15;
            predictions.Add("Oil filter may need replacement — no record of change found.");
        }

        if (mileage > 30000 && !partNames.Any(n => n.Contains("brake") && (n.Contains("pad") || n.Contains("pads"))))
        {
            score += 20;
            predictions.Add("Brake pads are due for inspection at this mileage.");
        }

        if (vehicleAge > 4 && !partNames.Any(n => n.Contains("battery")))
        {
            score += 15;
            predictions.Add("Battery replacement may be needed — no record found and vehicle is over 4 years old.");
        }

        if (mileage > 20000 && !partNames.Any(n => n.Contains("air") && n.Contains("filter")))
        {
            score += 10;
            predictions.Add("Air filter replacement recommended at this mileage.");
        }

        string riskLevel = score switch
        {
            <= 20 => "Low",
            <= 50 => "Moderate",
            <= 80 => "High",
            _ => "Critical"
        };

        return new VehiclePredictionDto
        {
            VehicleId = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            Make = vehicle.Make,
            Model = vehicle.Model,
            RiskScore = score,
            RiskLevel = riskLevel,
            Predictions = predictions,
            AnalysedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<VehiclePredictionDto>> AnalyseCustomerVehiclesAsync(int customerId)
    {
        var vehicleIds = await context.Vehicles
            .Where(v => v.CustomerId == customerId)
            .Select(v => v.Id)
            .ToListAsync();

        var results = new List<VehiclePredictionDto>();
        foreach (var id in vehicleIds)
        {
            results.Add(await AnalyseVehicleAsync(id));
        }
        return results;
    }
}
