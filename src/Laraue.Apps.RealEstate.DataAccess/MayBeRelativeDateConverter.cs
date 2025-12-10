using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Laraue.Apps.RealEstate.DataAccess;

public class MayBeRelativeDateConverter : ValueConverter<MayBeRelativeDate, string>
{
    public MayBeRelativeDateConverter() : base(
        x => x.SourceValue,
        x => new MayBeRelativeDate(x))
    {
    }
}

