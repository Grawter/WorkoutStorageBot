using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Extensions
{
    internal static class DBExtensions
    {
        internal static string GetDBProvider(this EntityContext db)
            => !string.IsNullOrWhiteSpace(db.Database.ProviderName) ? db.Database.ProviderName : throw new InvalidOperationException($"Не удалось название Db провайдера");

        internal static async Task<int> ExecuteSQL(this EntityContext db, string sql)
            => await db.Database.ExecuteSqlRawAsync(sql);
    }
}