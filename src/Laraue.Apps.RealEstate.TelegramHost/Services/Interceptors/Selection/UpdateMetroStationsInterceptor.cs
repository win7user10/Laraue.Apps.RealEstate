using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core.Utils;
using Laraue.Telegram.NET.Interceptors.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateMetroStationsInterceptor : BaseInterceptor<IList<long>?>
{
    private readonly AdvertisementsDbContext _context;

    public UpdateMetroStationsInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
        _context = context;
    }

    protected override Expression<Func<DataAccess.Models.Selection, IList<long>?>> FieldSelectorExpression => x => x.MetroIds;
    public override string FieldName => "Станции метро";

    protected override bool StayOnPreviousPageAfterEdit => true;

    protected override async Task<string> FormatTextAsync(IList<long>? value)
    {
        if (value is null)
        {
            return "Не заданы";
        }
        
        var stopNames = await _context.TransportStops
            .Where(x => value.Contains(x.Id))
            .Select(x => x.Name)
            .ToListAsync();

        return string.Join(", ", stopNames);
    }

    public override string Description => "Позволяет выбрать конкретные станции метро для выборки"; 
    
    protected override async Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<IList<long>?> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var callbackData = requestContext.Update.CallbackQuery?.Data;
        if (callbackData is null
            || !long.TryParse(callbackData, out var stopId)
            || !await _context.TransportStops.AnyAsync(x => x.Id == stopId))
        {
            interceptResult.SetError("Выберите станцию метро для включения/исключения из списка");
            return;
        }

        var selectionId = interceptorContext?.Id;
        var selectedIds = await _context.Selections
            .Where(x => x.Id == selectionId.GetValueOrDefault())
            .Select(x => x.MetroIds)
            .FirstAsync();

        selectedIds ??= new List<long>();
        if (!selectedIds.Remove(stopId))
        {
            selectedIds.Add(stopId);
        }

        if (selectedIds.Count == 0)
        {
            selectedIds = null;
        }
        
        interceptResult.SetResult(selectedIds);
    }

    protected override async Task FillMessageAsync(TelegramMessageBuilder messageBuilder, UpdateSelectionContext? context)
    {
        var stops = await _context.TransportStops
            .Select(x => new { x.Name, x.Id, x.Color })
            .ToListAsync();

        var sortedStops = stops
            .GroupBy(x => x.Color)
            .SelectMany(x => x);

        var selectionId = context?.Id;
        var selectedStopIds = (await _context.Selections
                .Where(x => x.Id == selectionId.GetValueOrDefault())
                .Select(x => x.MetroIds)
                .FirstAsync())
            ?.ToHashSet() ?? new HashSet<long>();

        foreach (var stopsChunk in sortedStops.Chunk(4))
        {
            messageBuilder.AddInlineKeyboardButtons(stopsChunk
                .Select(x =>
                {
                    var text = selectedStopIds.Contains(x.Id)
                        ? $"✅ {x.Name}"
                        : x.Name;

                    return InlineKeyboardButton
                        .WithCallbackData(text, x.Id.ToString());
                }));
        }
    }
}