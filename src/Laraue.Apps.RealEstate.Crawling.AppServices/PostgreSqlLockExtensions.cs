using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public static class PostgreSqlLockExtensions
{
    public static async Task<bool> ShareLockAsync<TEntity>(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        var model = context.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException("Entity data is not found");

        var sqlBuilder = new StringBuilder("LOCK TABLE ");
        var schemaName = model.GetSchema();
        if (schemaName != null)
        {
            sqlBuilder
                .Append('\"')
                .Append(schemaName)
                .Append('\"')
                .Append('.');
        }
        
        var tableName = model.GetTableName();
        sqlBuilder
            .Append('\"')
            .Append(tableName)
            .Append('\"')
            .Append(" IN SHARE MODE");
        
        var query = sqlBuilder.ToString();
        await context.Database.ExecuteSqlRawAsync(query, cancellationToken);
        
        return true;
    }
}