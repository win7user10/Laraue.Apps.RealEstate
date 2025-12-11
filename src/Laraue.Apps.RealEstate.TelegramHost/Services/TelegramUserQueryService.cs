using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Laraue.Telegram.NET.Authentication.Services;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public class TelegramUserQueryService(AdvertisementsDbContext context) : ITelegramUserQueryService<User, Guid>
{
    public Task<User?> FindAsync(long telegramId)
    {
        return context.Users
            .Where(u => u.TelegramId == telegramId)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateAsync(User user)
    {
        context.Users.Add(user);
        
        await context.SaveChangesAsync();
        
        return user.Id;
    }
}