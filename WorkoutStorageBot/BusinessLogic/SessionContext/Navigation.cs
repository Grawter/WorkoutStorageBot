#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class Navigation
    {
        internal MessageNavigationTarget MessageNavigationTarget { get; set; }
        internal QueryFrom QueryFrom { get; set; }

        internal void ResetMessageNavigationTarget()
        {
            this.MessageNavigationTarget = MessageNavigationTarget.Default;
        }

        internal void ResetQueryFrom()
        {
            this.QueryFrom = QueryFrom.NoMatter;
        }

        internal void ResetNavigation()
        {
            ResetMessageNavigationTarget();
            ResetQueryFrom();
        }
    }
}