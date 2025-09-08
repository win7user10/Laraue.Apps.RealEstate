namespace Laraue.Apps.RealEstate.IntegrationTests.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> getAsyncEnumerable)
    {
        var result = new List<T>();

        await foreach (var element in getAsyncEnumerable)
        {
            result.Add(element);
        }

        return result;
    }

#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
#pragma warning restore CS1998
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }
}