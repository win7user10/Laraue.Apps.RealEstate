using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Laraue.Apps.RealEstate.Db;

public class MayBeRelativeDateConverter : ValueConverter<MayBeRelativeDate, string>
{
    public MayBeRelativeDateConverter() : base(
        x => x.SourceValue,
        x => new MayBeRelativeDate(x))
    {
    }
}

