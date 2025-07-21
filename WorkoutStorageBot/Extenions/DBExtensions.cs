#region using

using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Extenions
{
    internal static class DBExtensions
    {
        internal static string GetDBProvider(this EntityContext db)
            => db.Database.ProviderName;

        internal static int ExecuteSQL(this EntityContext db, string sql)
            => db.Database.ExecuteSqlRaw(sql);
    }
}