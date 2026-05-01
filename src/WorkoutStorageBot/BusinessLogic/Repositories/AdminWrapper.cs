using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Repositories
{
    internal class AdminWrapper : UserInformationRepository
    {
        internal AdminWrapper(EntityContext db, ConfigurationData configurationData, ICustomLoggerFactory loggerFactory) : base(db)
        {
            this.configurationData = configurationData;

            this.logger = loggerFactory.CreateLogger<UserInformationRepository>();

            this.WhiteListIsEnable = configurationData.Bot.WhiteListIsEnable;
        }

        internal AdminWrapper(EntityContext db, ConfigurationData configurationData, ILogger logger) : base(db)
        {
            this.configurationData = configurationData;

            this.logger = logger;
        }

        internal bool WhiteListIsEnable { get; private set; }

        private readonly ConfigurationData configurationData;

        private readonly ILogger logger;

        private IEnumerable<string> OwnersChatIDs => configurationData.Bot.OwnersChatIDs;

        internal string GetSafeConfigurationData()
            => ConfigurationManager.GetSerializedSafeConfigurationData(this.configurationData);

        internal void ChangeWhiteListMode()
        {
            WhiteListIsEnable = !WhiteListIsEnable;

            AddLogAction($"Глобальный режим белого списка установлен в: {WhiteListIsEnable}");
        }

        internal async Task ChangeBlackListByUser(UserInformation user)
        {
            await base.ChangeBlackListByUser(user);

            AddLogAction($"Режим чёрного списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.BlackList}");
        }

        internal async Task ChangeWhiteListByUser(UserInformation user)
        {
            await base.ChangeWhiteListByUser(user);

            AddLogAction($"Режим белого списка для пользователя {user.Id} ({user.Username}-{user.UserId}) установлен в: {user.WhiteList}");
        }

        internal async Task DeleteAccount(UserInformation user)
        { 
            await base.DeleteAccount(user);

            AddLogAction($"Пользователь {user.Id} ({user.Username}-{user.UserId}) удалён");
        }

        internal async Task<UserInformation?> TryCreateNewUserInformation(User user)
        {
            if (!WhiteListIsEnable)
            {
                UserInformation newUser = await base.TryCreateNewUserInformation(user);

                AddLogAction($"Создан новый пользователь {newUser.Id} ({newUser.Username}-{newUser.UserId})");

                return newUser;
            }
            else
            {
                AddLogAction($"Не удалось создать нового пользователя {user.Id} ({user.Username})");
                return default;
            }
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
            => logger.LogWarning(message);
    }
}