using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ResetTempDomainsBF : ButtonsFactory
    {
        internal ResetTempDomainsBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string domainType = (additionalParameters?["type"]).ThrowIfNullOrWhiteSpace();

            switch (domainType)
            {
                case CommonConsts.DomainsAndEntities.Day:
                    throw new NotImplementedException($"Нереализовано для типа домена {CommonConsts.DomainsAndEntities.Day}, т.к. не нашлось ни одно небходимого кейса");

                case CommonConsts.DomainsAndEntities.Exercise:
                    AddInlineButton($"Сбросить добавленные упражнения", $"2|ResetTempDomains|{CommonConsts.DomainsAndEntities.Exercise}");

                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(domainType)}: {domainType}");
            }
        }
    }
}
