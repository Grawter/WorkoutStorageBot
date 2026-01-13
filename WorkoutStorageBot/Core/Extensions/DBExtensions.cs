using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Extensions
{
    internal static class DBExtensions
    {
        internal static string GetDBProvider(this EntityContext db)
            => db.Database.ProviderName;

        internal static async Task<int> ExecuteSQL(this EntityContext db, string sql)
            => await db.Database.ExecuteSqlRawAsync(sql);
    }
}