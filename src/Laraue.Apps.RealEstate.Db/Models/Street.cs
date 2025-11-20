using System.ComponentModel.DataAnnotations;

namespace Laraue.Apps.RealEstate.Db.Models;

public class Street
{
    public long Id { get; init; }
    public long CityId { get; init; }

    public City City { get; init; } = null!;

    [MaxLength(500)]
    public string Name { get; init; } = string.Empty;
}