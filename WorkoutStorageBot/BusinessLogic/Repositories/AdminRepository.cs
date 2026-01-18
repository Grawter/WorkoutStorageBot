using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class AdminRepository : CoreRepository
    {
        internal AdminRepository(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(AdminRepository))
        {
            ConfigurationData configurationData = CoreTools.ConfigurationData;

            Logger = CoreTools.LoggerFactory.CreateLogger<AdminRepository>();

            WhiteListIsEnable = configurationData.Bot.WhiteListIsEnable;
            OwnersChatIDs = configurationData.Bot.OwnersChatIDs;
        }

        internal bool WhiteListIsEnable { get; private set; }

        private IEnumerable<string> OwnersChatIDs { get; }

        private ILogger Logger { get; }

        internal string GetSafeConfigurationData()
            => ConfigurationManager.GetSerializedSafeDeepCopy(CoreTools.ConfigurationData);

        internal void ChangeWhiteListMode()
        {
            WhiteListIsEnable = !WhiteListIsEnable;

            AddLogAction($"Глобальный режим белого списка установлен в: {WhiteListIsEnable}");
        }

        internal async Task ChangeBlackListByUser(UserInformation user)
        {
            user.BlackList = !user.BlackList;

            CoreTools.Db.UsersInformation.Update(user);
            await CoreTools.Db.SaveChangesAsync();

            AddLogAction($"Режим чёрного списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.BlackList}");
        }

        internal async Task ChangeWhiteListByUser(UserInformation user)
        {
            user.WhiteList = !user.WhiteList;

            CoreTools.Db.UsersInformation.Update(user);
            await CoreTools.Db.SaveChangesAsync();

            AddLogAction($"Режим белого списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.WhiteList}");
        }

        internal async Task DeleteAccount(UserInformation user)
        {
            CoreTools.Db.UsersInformation.Remove(user);
            await CoreTools.Db.SaveChangesAsync();

            AddLogAction($"Пользователь {user.Id} ({user.Username}-{user.UserId}) удалён");
        }

        internal async Task<UserInformation?> GetUserInformationWithoutTracking(long userId)
            => await CoreTools.Db.UsersInformation.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

        internal async Task<UserInformation> GetRequiredUserInformation(long userId)
            => await GetUserInformation(userId) ?? throw new InvalidOperationException($"Пользователь с UserId = {userId} не найден!");

        internal async Task<UserInformation?> GetUserInformation(long userId)
            => await CoreTools.Db.UsersInformation.FirstOrDefaultAsync(u => u.UserId == userId);

        internal async Task<UserInformation> GetRequiredUserInformation(string userName)
            => await GetUserInformation(userName) ?? throw new InvalidOperationException($"Пользователь с Username = {userName} не найден!");

        internal async Task<UserInformation?> GetUserInformation(string userName)
            => await CoreTools.Db.UsersInformation.FirstOrDefaultAsync(u => u.Username == userName);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(long userId)
             => await GetFullUserInformationWithoutTracking(u => u.UserId == userId);

        internal async Task<UserInformation?> GetFullUserInformationWithoutTracking(string userName)
            => await GetFullUserInformationWithoutTracking(u => u.Username == userName);

        private async Task<UserInformation?> GetFullUserInformationWithoutTracking(Expression<Func<UserInformation, bool>> predicate)
        {
            return await CoreTools.Db.UsersInformation.AsNoTracking()
                                                      .Where(predicate)
                                                      .Include(u => u.Cycles)
                                                        .ThenInclude(c => c.Days)
                                                            .ThenInclude(d => d.Exercises)
                                                      .AsSplitQuery() // Разделение запросов, для оптимизации
                                                      .FirstOrDefaultAsync();
        }

        internal async Task<UserInformation?> TryCreateNewUserInformation(User user)
        {
            if (!WhiteListIsEnable)
            {
                UserInformation newUser = new UserInformation
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    Username = string.IsNullOrWhiteSpace(user.Username) ? "Empty" : $"@{user.Username}",
                    WhiteList = false,
                    BlackList = false
                };

                await CoreTools.Db.UsersInformation.AddAsync(newUser);
                await CoreTools.Db.SaveChangesAsync();

                return newUser;
            }
            else
                return default;
        }

        internal bool UserHasAccess(DTOUserInformation user)
            => UserHasAccess(new UserInformation() { UserId = user.UserId, FirstName = user.FirstName, Username = user.Username, BlackList = user.BlackList, WhiteList = user.WhiteList });

        internal bool UserHasAccess(UserInformation user)
        {
            if (UserIsOwner(user.UserId))
                return true;

            if (user.BlackList)
                return false;

            if (WhiteListIsEnable && !user.WhiteList)
                return false;

            return true;
        }

        internal bool UserIsOwner(long userID)
            => OwnersChatIDs.Contains(userID.ToString());

        private void AddLogAction(string message)
            => Logger.LogWarning(message);
    }
}