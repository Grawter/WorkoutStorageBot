using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Telegram.Bot.Types;
using WorkoutStorageBot.Core.Repositories.Abstraction;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class UserInformationRepository : CoreRepository
    {
        internal UserInformationRepository(EntityContext db) : base(db, nameof(UserInformationRepository))
        { }

        protected async Task ChangeBlackListByUser(UserInformation user, bool isNeedSaveChanges = true)
        {
            user.BlackList = !user.BlackList;

            db.UsersInformation.Update(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync();
        }

        protected async Task ChangeWhiteListByUser(UserInformation user, bool isNeedSaveChanges = true)
        {
            user.WhiteList = !user.WhiteList;

            db.UsersInformation.Update(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync();
        }

        protected async Task DeleteAccount(UserInformation user, bool isNeedSaveChanges = true)
        {
            db.UsersInformation.Remove(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync();
        }

        internal async Task<UserInformation?> GetUserInformationWithoutTracking(string userName)
            => await db.UsersInformation.AsNoTracking().FirstOrDefaultAsync(u => u.Username == userName);

        internal async Task<UserInformation?> GetUserInformationWithoutTracking(long userId)
            => await db.UsersInformation.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

        internal async Task<UserInformation> GetRequiredUserInformation(long userId)
            => await GetUserInformation(userId) ?? throw new InvalidOperationException($"Пользователь с UserId = {userId} не найден!");

        internal async Task<UserInformation?> GetUserInformation(long userId)
            => await db.UsersInformation.FirstOrDefaultAsync(u => u.UserId == userId);

        internal async Task<UserInformation> GetRequiredUserInformation(string userName)
            => await GetUserInformation(userName) ?? throw new InvalidOperationException($"Пользователь с Username = {userName} не найден!");

        internal async Task<UserInformation?> GetUserInformation(string userName)
            => await db.UsersInformation.FirstOrDefaultAsync(u => u.Username == userName);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(long userId)
             => await GetFullUserInformationWithoutTracking(u => u.UserId == userId);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(string userName)
            => await GetFullUserInformationWithoutTracking(u => u.Username == userName);

        private async Task<UserInformation?> GetFullUserInformationWithoutTracking(Expression<Func<UserInformation, bool>> predicate)
        {
            return await db.UsersInformation.AsNoTracking()
                                            .Where(predicate)
                                            .Include(u => u.Cycles)
                                            .ThenInclude(c => c.Days)
                                                .ThenInclude(d => d.Exercises)
                                            .AsSplitQuery() // Разделение запросов, для оптимизации
                                            .FirstOrDefaultAsync();
        }

        protected async Task<UserInformation?> TryCreateNewUserInformation(User user, bool isNeedSaveChanges = true)
        {
            UserInformation newUser = new UserInformation
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                Username = string.IsNullOrWhiteSpace(user.Username) ? "Empty" : $"@{user.Username}",
                WhiteList = false,
                BlackList = false
            };

            await db.UsersInformation.AddAsync(newUser);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync();

            return newUser;
        }
    }
}