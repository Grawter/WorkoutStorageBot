using FluentAssertions;
using Telegram.Bot.Types;
using WorkoutStorageBot.BusinessLogic.Helpers.Updates;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Updates
{
    public class UpdatesHelperTests
    {
        [Fact]
        public void GetUpdateInfo_WithEmptyUpdate_ShouldReturnUnknownUpdateInfo()
        {
            // Arrange
            Update update = new Update();

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.Unknown);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<UnknownUpdateInfo>();
        }

        [Fact]
        public void GetUpdateInfo_WithEmptyMessage_ShouldReturnUnknownUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.Message = new Message();

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.Message);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<UnknownUpdateInfo>();
        }

        [Fact]
        public void GetUpdateInfo_WithMessageWithUserWithChatWithEmptyChatId_ShouldThrowArgumentNullException()
        {
            // Arrange
            Update update = new Update();
            update.Message = new Message();
            update.Message.From = new User();
            update.Message.Chat = new Chat();

            // Act
            Action act = () => UpdatesHelper.GetUpdateInfo(update);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetUpdateInfo_WithMessageWithUserWithChatWithChatIdWithNotMessageText_ShouldReturnUnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.Message = new Message();
            update.Message.From = new User();
            update.Message.Chat = new Chat();
            update.Message.Chat.Id = 1;
            update.Message.Photo = new PhotoSize[1];

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.Message);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.Message.From);
            shortUpdateInfo.ChatId.Should().Be(update.Message.Chat.Id);
            shortUpdateInfo.Data.Should().Be("Unknown or empty data");
        }

        [Fact]
        public void GetUpdateInfo_WithMessageWithUserWithChatWithChatIdWithEmptyStringMessageText_ShouldReturnUnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.Message = new Message();
            update.Message.From = new User();
            update.Message.Chat = new Chat();
            update.Message.Chat.Id = 1;
            update.Message.Text = string.Empty;

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.Message);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.Message.From);
            shortUpdateInfo.ChatId.Should().Be(update.Message.Chat.Id);
            shortUpdateInfo.Data.Should().Be("Unknown or empty data");
        }

        [Fact]
        public void GetUpdateInfo_WithMessageWithUserWithChatWithChatIdWithStringMessageText_ShouldReturnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.Message = new Message();
            update.Message.From = new User();
            update.Message.Chat = new Chat();
            update.Message.Chat.Id = 1;
            update.Message.Text = "TestData";

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.Message);
            updateInfo.IsExpectedType.Should().BeTrue();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.Message.From);
            shortUpdateInfo.ChatId.Should().Be(update.Message.Chat.Id);
            shortUpdateInfo.Data.Should().Be(update.Message.Text);
        }

        [Fact]
        public void GetUpdateInfo_WithEmptyCallbackQuery_ShouldReturnUnknownUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.CallbackQuery = new CallbackQuery();

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.CallbackQuery);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<UnknownUpdateInfo>();
        }

        [Fact]
        public void GetUpdateInfo_WithCallbackQueryWithEmptyUser_ShouldReturnUnknownUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.CallbackQuery = new CallbackQuery();
            update.CallbackQuery.From = new User();

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.CallbackQuery);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<UnknownUpdateInfo>();
        }

        [Fact]
        public void GetUpdateInfo_WithCallbackQueryWithUserWithChatWithEmptyChatId_ShouldThrowArgumentNullException()
        {
            // Arrange
            Update update = new Update();
            update.CallbackQuery = new CallbackQuery();
            update.CallbackQuery.From = new User();
            update.CallbackQuery.Message = new Message();
            update.CallbackQuery.Message.Chat = new Chat();

            // Act
            Action act = () => UpdatesHelper.GetUpdateInfo(update);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetUpdateInfo_WithCallbackQueryWithUserWithChatWithChatIdWithEmptyCallbackQueryData_ShouldReturnUnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.CallbackQuery = new CallbackQuery();
            update.CallbackQuery.From = new User();
            update.CallbackQuery.Message = new Message();
            update.CallbackQuery.Message.Chat = new Chat();
            update.CallbackQuery.Message.Chat.Id = 1;
            update.CallbackQuery.Data = string.Empty;

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.CallbackQuery);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.CallbackQuery.From);
            shortUpdateInfo.ChatId.Should().Be(update.CallbackQuery.Message.Chat.Id);
            shortUpdateInfo.Data.Should().Be("Unknown or empty data");
        }

        [Fact]
        public void GetUpdateInfo_WithCallbackQueryWithUserWithChatWithChatIdWithCallbackQueryData_ShouldReturnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.CallbackQuery = new CallbackQuery();
            update.CallbackQuery.From = new User();
            update.CallbackQuery.Message = new Message();
            update.CallbackQuery.Message.Chat = new Chat();
            update.CallbackQuery.Message.Chat.Id = 1;
            update.CallbackQuery.Data = "Test callBackData";

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.CallbackQuery);
            updateInfo.IsExpectedType.Should().BeTrue();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.CallbackQuery.From);
            shortUpdateInfo.ChatId.Should().Be(update.CallbackQuery.Message.Chat.Id);
            shortUpdateInfo.Data.Should().Be(update.CallbackQuery.Data);
        }

        [Fact]
        public void GetUpdateInfo_WithEmptyEditedMessage_ShouldReturnUnknownUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.EditedMessage = new Message();

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.EditedMessage);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<UnknownUpdateInfo>();
        }

        [Fact]
        public void GetUpdateInfo_WithEmptyEditedMessageWithFromWithChatWithEmptyChatId_ShouldThrowArgumentNullException()
        {
            // Arrange
            Update update = new Update();
            update.EditedMessage = new Message();
            update.EditedMessage.From = new User();
            update.EditedMessage.Chat = new Chat();

            // Act
            Action act = () => UpdatesHelper.GetUpdateInfo(update);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }


        [Fact]
        public void GetUpdateInfo_WithEmptyEditedMessageWithFromWithChatWithChatIdWithEmptyValue_ShouldReturnUnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.EditedMessage = new Message();
            update.EditedMessage.From = new User();
            update.EditedMessage.Chat = new Chat();
            update.EditedMessage.Chat.Id = 1;

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.EditedMessage);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.EditedMessage.From);
            shortUpdateInfo.ChatId.Should().Be(update.EditedMessage.Chat.Id);
            shortUpdateInfo.Data.Should().Be("Unknown or empty data");
        }

        [Fact]
        public void GetUpdateInfo_WithEmptyEditedMessageWithFromWithChatWithChatIdWithStringMessageText_ShouldReturnUnExpectedShortUpdateInfo()
        {
            // Arrange
            Update update = new Update();
            update.EditedMessage = new Message();
            update.EditedMessage.From = new User();
            update.EditedMessage.Chat = new Chat();
            update.EditedMessage.Chat.Id = 1;
            update.EditedMessage.Text = "Test";

            // Act
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            // Assert
            updateInfo.Update.Should().Be(update);
            updateInfo.UpdateType.Should().Be(Telegram.Bot.Types.Enums.UpdateType.EditedMessage);
            updateInfo.IsExpectedType.Should().BeFalse();
            updateInfo.Should().BeOfType<ShortUpdateInfo>();

            ShortUpdateInfo shortUpdateInfo = (ShortUpdateInfo)updateInfo;

            shortUpdateInfo.User.Should().Be(update.EditedMessage.From);
            shortUpdateInfo.ChatId.Should().Be(update.EditedMessage.Chat.Id);
            shortUpdateInfo.Data.Should().Be(update.EditedMessage.Text);
        }
    }
}