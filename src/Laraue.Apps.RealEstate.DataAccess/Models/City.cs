using System.ComponentModel.DataAnnotations;

namespace Laraue.Apps.RealEstate.DataAccess.Models;

public class City
{
    public long Id { get; init; }
    
    [MaxLength(200)]
    public required string Name { get; init; }
}