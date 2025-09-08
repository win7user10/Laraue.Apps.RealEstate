using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Date;
using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Interval;
using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateInterceptorsFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _interceptors = new ();
    private readonly Dictionary<int, IList<Type>> _interceptorsByGroup = new ();

    public UpdateInterceptorsFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        AddInterceptor<UpdateSelectionNameInterceptor>(1);
        AddInterceptor<SelectionIntervalInterceptor>(1);
        AddInterceptor<UpdateMinPriceInterceptor>(2);
        AddInterceptor<UpdateMaxPriceInterceptor>(2);
        AddInterceptor<UpdateMinSquareMeterPriceInterceptor>(3);
        AddInterceptor<UpdateMaxSquareMeterPriceInterceptor>(3);
        AddInterceptor<MinDateInterceptor>(4);
        AddInterceptor<MaxDateInterceptor>(4);
        AddInterceptor<UpdateMetroStationsInterceptor>(5);
        AddInterceptor<UpdateSortingFieldInterceptor>(6);
        AddInterceptor<UpdateSortingDirectionInterceptor>(6);
        AddInterceptor<UpdateLimitInterceptor>(7);
    }

    private void AddInterceptor<T>(int row) where T : class, IUpdateInterceptor, IRequestInterceptor<UpdateSelectionContext>
    {
        _interceptors.Add(typeof(T).Name, typeof(T));

        _interceptorsByGroup.TryAdd(row, new List<Type>());
        _interceptorsByGroup[row].Add(typeof(T));
    }
    
    public IRequestInterceptor<UpdateSelectionContext> Get(string key)
    {
        if (_interceptors.TryGetValue(key, out var interceptorType))
        {
            return (_serviceProvider.GetRequiredService(interceptorType)
                as IRequestInterceptor<UpdateSelectionContext>)!;
        }

        throw new InvalidOperationException($"Wrong interceptor key {key}");
    }

    public IEnumerable<IEnumerable<IUpdateInterceptor>> All()
    {
        return _interceptorsByGroup
            .Select(interceptorGroup => interceptorGroup
                .Value
                .Select(interceptor => (_serviceProvider.GetRequiredService(interceptor)
                    as IUpdateInterceptor)!));
    }
}