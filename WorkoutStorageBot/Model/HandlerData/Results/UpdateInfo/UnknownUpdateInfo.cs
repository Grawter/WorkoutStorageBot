﻿#region using

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Common;

#endregion

namespace WorkoutStorageBot.Model.HandlerData.Results.UpdateInfo
{
    internal class UnknownUpdateInfo : IUpdateInfo
    {
        internal UnknownUpdateInfo(Update update, UpdateType updateType)
        {
            Update = CommonHelper.GetIfNotNull(update);
            UpdateType = updateType;
            IsExpectedType = false;
        }

        public UpdateType UpdateType { get; }

        public bool IsExpectedType { get; }

        public Update Update { get; }
    }
}