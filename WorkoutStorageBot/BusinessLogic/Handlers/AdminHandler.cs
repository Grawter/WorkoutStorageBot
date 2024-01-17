#region using
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers
{
    internal class AdminHandler
    {
        private readonly EntityContext db;

        internal AdminHandler(EntityContext db)
        {
            this.db = db;
        }

        internal void ChangeBlackList(UserInformation user)
        {
            user.BlackList = !user.BlackList;

            db.UsersInformation.Update(user);
            db.SaveChanges();
        }

        internal void ChangeWhiteList(UserInformation user)
        {
            user.WhiteList = !user.WhiteList;

            db.UsersInformation.Update(user);
            db.SaveChanges();
        }

        internal void DeleteAccount(UserInformation user)
        {
            db.UsersInformation.Remove(user);
            db.SaveChanges();
        }

        internal UserInformation? GetUserInformation(long userId)
        {
            return db.UsersInformation.FirstOrDefault(u => u.UserId == userId);
        }

    }
}