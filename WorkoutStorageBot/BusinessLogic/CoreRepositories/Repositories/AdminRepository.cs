#region using

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.DomainsAndEntities;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories
{
    internal class AdminRepository : CoreRepository
    {
        internal AdminRepository(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(AdminRepository))
        {
            ConfigurationData configurationData = CommonHelper.GetIfNotNull(coreTools.ConfigurationData);

            Logger = CommonHelper.GetIfNotNull(coreTools.LoggerFactory).CreateLogger<AdminRepository>();

            WhiteListIsEnable = configurationData.Bot.WhiteListIsEnable;
            OwnersChatIDs = configurationData.Bot.OwnersChatIDs;

            SaveConfigurationData = ConfigurationManager.GetSerializedSaveDeepCopy(configurationData);
        }

        internal AdminRepository(CoreTools coreTools) : base(coreTools, nameof(AdminRepository))
        {
            ConfigurationData configurationData = CommonHelper.GetIfNotNull(coreTools.ConfigurationData);

            Logger = CommonHelper.GetIfNotNull(coreTools.LoggerFactory).CreateLogger<AdminRepository>();

            WhiteListIsEnable = configurationData.Bot.WhiteListIsEnable;
            OwnersChatIDs = configurationData.Bot.OwnersChatIDs;

            SaveConfigurationData = ConfigurationManager.GetSerializedSaveDeepCopy(configurationData);
        }

        internal bool WhiteListIsEnable { get; private set; }

        private string[] OwnersChatIDs { get; }

        internal string SaveConfigurationData { get;} 

        private ILogger<AdminRepository> Logger { get; }

        internal void ChangeWhiteListMode()
        {
            WhiteListIsEnable = !WhiteListIsEnable;

            AddLogAction($"Глобальный режим белого списка установлен в: {WhiteListIsEnable}");
        }

        internal void ChangeBlackListByUser(UserInformation user)
        {
            user.BlackList = !user.BlackList;

            CoreTools.Db.UsersInformation.Update(user);
            CoreTools.Db.SaveChanges();

            AddLogAction($"Режим чёрного списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.BlackList}");
        }

        internal void ChangeWhiteListByUser(UserInformation user)
        {
            user.WhiteList = !user.WhiteList;

            CoreTools.Db.UsersInformation.Update(user);
            CoreTools.Db.SaveChanges();

            AddLogAction($"Режим белого списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.WhiteList}");
        }

        internal void DeleteAccount(UserInformation user)
        {
            CoreTools.Db.UsersInformation.Remove(user);
            CoreTools.Db.SaveChanges();

            AddLogAction($"Пользователь {user.Id} ({user.Username}-{user.UserId}) удалён");
        }

        internal UserInformation GetRequiredUserInformation(long userId)
            => GetUserInformation(userId) ?? throw new InvalidOperationException($"Пользователь с UserId = {userId} не найден!");
        
        internal UserInformation? GetUserInformation(long userId, bool throwNotFoundEx = false)
            => CoreTools.Db.UsersInformation.FirstOrDefault(u => u.UserId == userId);

        internal UserInformation GetRequiredUserInformation(string userName)
            => GetUserInformation(userName) ?? throw new InvalidOperationException($"Пользователь с Username = {userName} не найден!");

        internal UserInformation? GetUserInformation(string userName, bool throwNotFoundEx = false)
            => CoreTools.Db.UsersInformation.FirstOrDefault(u => u.Username == userName);

        internal UserInformation? GetFullUserInformation(long userID)
        {
            return CoreTools.Db.UsersInformation.Where(u => u.UserId == userID)
                                                    .Include(u => u.Cycles)
                                                        .ThenInclude(c => c.Days)
                                                            .ThenInclude(d => d.Exercises)
                                                .FirstOrDefault();
        }

        internal UserInformation? GetFullUserInformation(string userName)
        {
            return CoreTools.Db.UsersInformation.Where(u => u.Username == userName)
                                                    .Include(u => u.Cycles)
                                                        .ThenInclude(c => c.Days)
                                                            .ThenInclude(d => d.Exercises)
                                                .FirstOrDefault();
        }

        internal UserInformation CreateNewUser(User user)
        {
            if (!WhiteListIsEnable)
            {
                UserInformation newUser = new UserInformation { UserId = user.Id, Firstname = user.FirstName, Username = "@" + user.Username, WhiteList = false, BlackList = false };
                CoreTools.Db.UsersInformation.Add(newUser);
                CoreTools.Db.SaveChanges();

                return newUser;
            }
            else
                return default;
        }

        internal bool UserHasAccess(UserInformation user)
        {
            if (user == null)
                return false;

            if (UserIsOwner(user))
                return true;

            if (user.BlackList)
                return false;

            if (WhiteListIsEnable && !user.WhiteList)
                return false;

            return true;
        }

        internal bool UserIsOwner(UserInformation user)
            => OwnersChatIDs.Contains(user.UserId.ToString());

        private void AddLogAction(string message)
            => Logger.LogWarning(message);
    }
}