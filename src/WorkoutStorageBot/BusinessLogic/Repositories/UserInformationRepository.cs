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

        protected async Task ChangeBlackListByUser(UserInformation user, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            user.BlackList = !user.BlackList;

            db.UsersInformation.Update(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        protected async Task ChangeWhiteListByUser(UserInformation user, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            user.WhiteList = !user.WhiteList;

            db.UsersInformation.Update(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        protected async Task DeleteAccount(UserInformation user, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            db.UsersInformation.Remove(user);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        internal async Task<UserInformation?> GetUserInformationWithoutTracking(string userName, CancellationToken cancellationToken)
            => await db.UsersInformation.AsNoTracking().FirstOrDefaultAsync(u => u.Username == userName, cancellationToken);

        internal async Task<UserInformation?> GetUserInformationWithoutTracking(long userId, CancellationToken cancellationToken)
            => await db.UsersInformation.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        internal async Task<UserInformation> GetRequiredUserInformation(long userId, CancellationToken cancellationToken)
            => await GetUserInformation(userId, cancellationToken) ?? throw new InvalidOperationException($"Пользователь с UserId = {userId} не найден!");

        internal async Task<UserInformation?> GetUserInformation(long userId, CancellationToken cancellationToken)
            => await db.UsersInformation.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        internal async Task<UserInformation> GetRequiredUserInformation(string userName, CancellationToken cancellationToken)
            => await GetUserInformation(userName, cancellationToken) ?? throw new InvalidOperationException($"Пользователь с Username = {userName} не найден!");

        internal async Task<UserInformation?> GetUserInformation(string userName, CancellationToken cancellationToken)
            => await db.UsersInformation.FirstOrDefaultAsync(u => u.Username == userName, cancellationToken);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(long userId, CancellationToken cancellationToken)
             => await GetFullUserInformationWithoutTracking(u => u.UserId == userId, cancellationToken);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(string userName, CancellationToken cancellationToken)
            => await GetFullUserInformationWithoutTracking(u => u.Username == userName, cancellationToken);

        private async Task<UserInformation?> GetFullUserInformationWithoutTracking(Expression<Func<UserInformation, bool>> predicate, CancellationToken cancellationToken)
        {
            return await db.UsersInformation.AsNoTracking()
                                            .Where(predicate)
                                            .Include(u => u.Cycles)
                                            .ThenInclude(c => c.Days)
                                                .ThenInclude(d => d.Exercises)
                                            .AsSplitQuery() // Разделение запросов, для оптимизации
                                            .FirstOrDefaultAsync(cancellationToken);
        }

        protected async Task<UserInformation> TryCreateNewUserInformation(User user, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            UserInformation newUser = new UserInformation
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                Username = string.IsNullOrWhiteSpace(user.Username) ? "Empty" : $"@{user.Username}",
                WhiteList = false,
                BlackList = false
            };

            await db.UsersInformation.AddAsync(newUser, cancellationToken);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);

            return newUser;
        }
    }
}