using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using FluentAssertions;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.Session
{
    public class NavigationTests
    {
        [Fact]
        public void SetMessageNavigationTarget_ShouldSetSpecifiedMessageNavigationTarget()
        {
            // Arrange 
            Navigation navigation = new Navigation();

            // Act
            navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);

            // Assert
            navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameDay);
        }

        [Fact]
        public void SetQueryFrom_ShouldSetSpecifiedQueryFrom()
        {
            // Arrange 
            Navigation navigation = new Navigation();

            // Act
            navigation.SetQueryFrom(QueryFrom.Settings);

            // Assert
            navigation.QueryFrom.Should().Be(QueryFrom.Settings);
        }

        [Fact]
        public void SetMessageNavigationTargetAndQueryFrom_ShouldSetSpecifiedOptions()
        {
            // Arrange 
            Navigation navigation = new Navigation();

            // Act
            navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);
            navigation.SetQueryFrom(QueryFrom.Settings);

            // Assert
            navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameDay);
        }

        [Fact]
        public void ResetMessageNavigationTarget_ShouldResetMessageNavigationTargetToDefault()
        {
            // Arrange 
            Navigation navigation = new Navigation();
            navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);

            // Act
            navigation.ResetMessageNavigationTarget();

            // Assert
            navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
        }

        [Fact]
        public void ResetQueryFrom_ShouldResetQueryFromToDefault()
        {
            // Arrange 
            Navigation navigation = new Navigation();
            navigation.SetQueryFrom(QueryFrom.Settings);

            // Act
            navigation.ResetQueryFrom();

            // Assert
            navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
        }

        [Fact]
        public void ResetNavigation_ShouldResetNavigationToDefault()
        {
            // Arrange 
            Navigation navigation = new Navigation();
            navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);
            navigation.SetQueryFrom(QueryFrom.Settings);

            // Act
            navigation.ResetNavigation();

            // Assert
            navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);
            navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
        }
    }
}