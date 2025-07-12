using FluentAssertions;
using RabbitMqEventConsumer;

namespace RabbitMqEventConsumer.Tests;

public class JsonReplacementConfigTests
{
    [Fact]
    public void JsonReplacementConfig_ShouldHaveDefaultValues()
    {
        // Act
        var config = new JsonReplacementConfig();

        // Assert
        config.EnableReplacements.Should().BeFalse();
        config.ShowOriginalMessage.Should().BeTrue();
        config.ShowProcessedMessage.Should().BeTrue();
        config.Rules.Should().NotBeNull();
        config.Rules.Should().BeEmpty();
    }

    [Fact]
    public void JsonReplacementConfig_ShouldAllowSettingProperties()
    {
        // Arrange
        var config = new JsonReplacementConfig();
        var rule = new JsonReplacementRule 
        { 
            JsonPath = "user.email", 
            Placeholder = "[EMAIL]", 
            Enabled = true,
            Description = "Replace email addresses"
        };

        // Act
        config.EnableReplacements = true;
        config.ShowOriginalMessage = false;
        config.ShowProcessedMessage = false;
        config.Rules.Add(rule);

        // Assert
        config.EnableReplacements.Should().BeTrue();
        config.ShowOriginalMessage.Should().BeFalse();
        config.ShowProcessedMessage.Should().BeFalse();
        config.Rules.Should().HaveCount(1);
        config.Rules.First().Should().BeEquivalentTo(rule);
    }
}

public class JsonReplacementRuleTests
{
    [Fact]
    public void JsonReplacementRule_ShouldHaveDefaultValues()
    {
        // Act
        var rule = new JsonReplacementRule();

        // Assert
        rule.JsonPath.Should().Be(string.Empty);
        rule.Placeholder.Should().Be(string.Empty);
        rule.Enabled.Should().BeTrue();
        rule.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void JsonReplacementRule_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var rule = new JsonReplacementRule
        {
            JsonPath = "user.profile.email",
            Placeholder = "[REDACTED_EMAIL]",
            Enabled = false,
            Description = "Redact user email addresses"
        };

        // Assert
        rule.JsonPath.Should().Be("user.profile.email");
        rule.Placeholder.Should().Be("[REDACTED_EMAIL]");
        rule.Enabled.Should().BeFalse();
        rule.Description.Should().Be("Redact user email addresses");
    }

    [Theory]
    [InlineData("", "", true, "")]
    [InlineData("user.email", "[EMAIL]", false, "Email redaction")]
    [InlineData("users[0].phone", "[PHONE]", true, "Phone number masking")]
    public void JsonReplacementRule_ShouldSupportDifferentConfigurations(
        string jsonPath, 
        string placeholder, 
        bool enabled, 
        string description)
    {
        // Act
        var rule = new JsonReplacementRule
        {
            JsonPath = jsonPath,
            Placeholder = placeholder,
            Enabled = enabled,
            Description = description
        };

        // Assert
        rule.JsonPath.Should().Be(jsonPath);
        rule.Placeholder.Should().Be(placeholder);
        rule.Enabled.Should().Be(enabled);
        rule.Description.Should().Be(description);
    }
}
