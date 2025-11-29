using System.Text;

namespace Laraue.Apps.RealEstate.Abstractions.Extensions;

public static class AdvertisementDtoExtensions
{
    public static StringBuilder ToTelegramString(this IEnumerable<AdvertisementDto> advertisements)
    {
        var result = new StringBuilder();
        
        var advNumber = 0;
        foreach (var advertisement in advertisements)
        {
            result.Append($"{++advNumber}. ");
            result.Append(advertisement.ToTelegramString());
        }

        if (result.Length == 0)
        {
            result.Append("Объявления по заданным критериям не найдены");
        }

        return result;
    }

    private static StringBuilder ToTelegramString(this AdvertisementDto advertisement)
    {
        var result = new StringBuilder();
        
        result.AppendLine($"{advertisement.Link}");
        result.AppendLine($"Цена: {advertisement.TotalPrice.ToHumanReadableCurrencyString()}");
        result.AppendLine($"Цена за кв.м: {advertisement.SquareMeterPrice.ToHumanReadableCurrencyString()}");
        result.AppendLine($"Площадь: {advertisement.Square} кв.м");
        result.AppendLine($"Этаж: {advertisement.FloorNumber}/{advertisement.TotalFloorsNumber}");
        result.AppendLine($"Оценка ремонта: {advertisement.RenovationRating}/10");
        result.AppendLine($"Обновлено: {advertisement.UpdatedAt:G}");
        
        result.AppendLine("Метро:");
            
        foreach (var metroStation in advertisement.MetroStations)
        {
            result.AppendLine(GetMetroDescription(metroStation));
        }
            
        result.AppendLine();

        return result;
    }
    
    private static string GetMetroDescription(AdvertisementMetroStationDto stationDto)
    {
        var distanceDescription = stationDto.DistanceType == DistanceType.Car
            ? "🚗"
            : "🚶";

        var metroSymbol = GetMetroSymbol(stationDto.Color);

        return $"{metroSymbol}{stationDto.Name} - {stationDto.DistanceInMinutes} мин {distanceDescription}";
    }

    private static string GetMetroSymbol(string hexColor)
    {
        return string.Empty;
        
        return hexColor switch
        {
            "#cf0000" => "🔴",
            "#03238b" => "🔵",
            "#00701a" => "🟢",
            "#94007c" => "🟣",
            "#ff7f00" => "🟠",
            _ => throw new ArgumentException("Invalid color passed", nameof(hexColor)),
        };
    }
}