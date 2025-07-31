#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class Navigation
    {
        internal MessageNavigationTarget MessageNavigationTarget { get; private set; }
        internal QueryFrom QueryFrom { get; private set; }

        internal void SetMessageNavigationTarget(MessageNavigationTarget messageNavigationTarget)
        {
            MessageNavigationTarget = messageNavigationTarget;
        }

        internal void SetQueryFrom(QueryFrom queryFrom)
        {
            QueryFrom = queryFrom;
        }

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