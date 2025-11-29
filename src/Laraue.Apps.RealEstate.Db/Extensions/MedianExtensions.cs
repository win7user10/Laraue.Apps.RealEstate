using System.Numerics;

namespace Laraue.Apps.RealEstate.Db.Extensions;

public static class MedianExtensions
{
    public static float Median(this IEnumerable<float> source)
        => Median<float, float>(source);

    public static decimal Median(this IEnumerable<decimal> source)
        => Median<decimal, decimal>(source);

    public static double Median<TSource>(this IEnumerable<TSource> source)
        where TSource : struct, INumber<TSource>
        => Median<TSource, double>(source);

    public static TResult Median<TSource, TResult>(this IEnumerable<TSource> source)
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        var list = source.ToList();
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }
        list.Sort();
        var index = list.Count / 2;
        var value = TResult.CreateChecked(list[index]);
        if (list.Count % 2 == 1)
        {
            return value;
        }
        value += TResult.CreateChecked(list[index - 1]);
        return value / TResult.CreateChecked(2);
    }
}