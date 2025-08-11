using FluentAssertions;
using System.Text.Json;
using RabbitMqEventConsumer;

namespace RabbitMqEventConsumer.Tests;

public class JsonReplacementServiceTests
{
    private readonly JsonReplacementService _service;
    private readonly JsonReplacementConfig _config;

    public JsonReplacementServiceTests()
    {
        _config = new JsonReplacementConfig
        {
            EnableReplacements = true,
            ShowOriginalMessage = true,
            ShowProcessedMessage = true,
            Rules = new List<JsonReplacementRule>()
        };
        _service = new JsonReplacementService(_config);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithConfig()
    {
        // Arrange
        var config = new JsonReplacementConfig();

        // Act
        var service = new JsonReplacementService(config);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ProcessMessage_WhenReplacementsDisabled_ShouldReturnOriginalMessage()
    {
        // Arrange
        _config.EnableReplacements = false;
        var originalMessage = """{"user": {"email": "test@example.com"}}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        result.processedMessage.Should().Be(originalMessage);
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WhenNotJson_ShouldReturnOriginalMessage()
    {
        // Arrange
        var originalMessage = "This is not JSON";

        // Act
        var result = _service.ProcessMessage(originalMessage, false);

        // Assert
        result.processedMessage.Should().Be(originalMessage);
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WhenNoEnabledRules_ShouldReturnOriginalMessage()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "user.email", 
            Placeholder = "[EMAIL]", 
            Enabled = false 
        });
        var originalMessage = """{"user": {"email": "test@example.com"}}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        result.processedMessage.Should().Be(originalMessage);
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WithSimpleJsonPath_ShouldReplaceValue()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "user.email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """{"user": {"email": "test@example.com"}}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement.GetProperty("user").GetProperty("email").GetString().Should().Be("[EMAIL]");
        result.appliedRules.Should().Contain("user.email → [EMAIL]");
    }

    [Fact]
    public void ProcessMessage_WithMultipleRules_ShouldApplyAllEnabledRules()
    {
        // Arrange
        _config.Rules.AddRange(new[]
        {
            new JsonReplacementRule { JsonPath = "user.email", Placeholder = "[EMAIL]", Enabled = true },
            new JsonReplacementRule { JsonPath = "user.name", Placeholder = "[NAME]", Enabled = true },
            new JsonReplacementRule { JsonPath = "user.phone", Placeholder = "[PHONE]", Enabled = false }
        });
        var originalMessage = """{"user": {"email": "test@example.com", "name": "John Doe", "phone": "+1234567890"}}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement.GetProperty("user").GetProperty("email").GetString().Should().Be("[EMAIL]");
        processedJson.RootElement.GetProperty("user").GetProperty("name").GetString().Should().Be("[NAME]");
        processedJson.RootElement.GetProperty("user").GetProperty("phone").GetString().Should().Be("+1234567890");
        result.appliedRules.Should().HaveCount(2);
        result.appliedRules.Should().Contain("user.email → [EMAIL]");
        result.appliedRules.Should().Contain("user.name → [NAME]");
    }

    [Fact]
    public void ProcessMessage_WithArrayIndex_ShouldReplaceArrayElement()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "users[0].email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """{"users": [{"email": "first@example.com"}, {"email": "second@example.com"}]}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement.GetProperty("users")[0].GetProperty("email").GetString().Should().Be("[EMAIL]");
        processedJson.RootElement.GetProperty("users")[1].GetProperty("email").GetString().Should().Be("second@example.com");
        result.appliedRules.Should().Contain("users[0].email → [EMAIL]");
    }

    [Fact]
    public void ProcessMessage_WithDirectArrayAccess_ShouldReplaceArrayElement()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "emails[1]", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """{"emails": ["first@example.com", "second@example.com", "third@example.com"]}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement.GetProperty("emails")[0].GetString().Should().Be("first@example.com");
        processedJson.RootElement.GetProperty("emails")[1].GetString().Should().Be("[EMAIL]");
        processedJson.RootElement.GetProperty("emails")[2].GetString().Should().Be("third@example.com");
        result.appliedRules.Should().Contain("emails[1] → [EMAIL]");
    }

    [Fact]
    public void ProcessMessage_WithInvalidJsonPath_ShouldSkipRule()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "nonexistent.path", 
            Placeholder = "[PLACEHOLDER]", 
            Enabled = true 
        });
        var originalMessage = """{"user": {"email": "test@example.com"}}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        result.processedMessage.Should().Contain("test@example.com");
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WithInvalidArrayIndex_ShouldSkipRule()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "users[10].email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """{"users": [{"email": "first@example.com"}]}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        result.processedMessage.Should().Contain("first@example.com");
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WithInvalidJson_ShouldReturnOriginalMessage()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "user.email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var invalidJson = """{"user": {"email": "test@example.com}"""; // Missing closing quote and brace

        // Act
        var result = _service.ProcessMessage(invalidJson, true);

        // Assert
        result.processedMessage.Should().Be(invalidJson);
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WithNestedComplexPath_ShouldReplaceNestedValue()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "data.user.profile.personal.email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """
        {
            "data": {
                "user": {
                    "profile": {
                        "personal": {
                            "email": "deep@example.com"
                        }
                    }
                }
            }
        }
        """;

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        var email = processedJson.RootElement
            .GetProperty("data")
            .GetProperty("user")
            .GetProperty("profile")
            .GetProperty("personal")
            .GetProperty("email")
            .GetString();
        email.Should().Be("[EMAIL]");
        result.appliedRules.Should().Contain("data.user.profile.personal.email → [EMAIL]");
    }

    [Fact]
    public void ExtractJsonPaths_WithSimpleObject_ShouldReturnAllPaths()
    {
        // Arrange
        var jsonMessage = """{"user": {"name": "John", "email": "john@example.com"}}""";

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(jsonMessage);

        // Assert
        paths.Should().Contain("user");
        paths.Should().Contain("user.name");
        paths.Should().Contain("user.email");
        paths.Should().HaveCount(3);
    }

    [Fact]
    public void ExtractJsonPaths_WithArray_ShouldReturnArrayPaths()
    {
        // Arrange
        var jsonMessage = """{"users": [{"name": "John"}, {"name": "Jane"}]}""";

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(jsonMessage);

        // Assert
        paths.Should().Contain("users");
        paths.Should().Contain("users[0]");
        paths.Should().Contain("users[0].name");
        paths.Should().Contain("users[1]");
        paths.Should().Contain("users[1].name");
    }

    [Fact]
    public void ExtractJsonPaths_WithComplexNestedStructure_ShouldReturnAllPaths()
    {
        // Arrange
        var jsonMessage = """
        {
            "company": {
                "departments": [
                    {
                        "name": "IT",  
                        "employees": [
                            {"name": "John", "email": "john@company.com"}
                        ]
                    }
                ]
            }
        }
        """;

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(jsonMessage);

        // Assert
        paths.Should().Contain("company");
        paths.Should().Contain("company.departments");
        paths.Should().Contain("company.departments[0]");
        paths.Should().Contain("company.departments[0].name");
        paths.Should().Contain("company.departments[0].employees");
        paths.Should().Contain("company.departments[0].employees[0]");
        paths.Should().Contain("company.departments[0].employees[0].name");
        paths.Should().Contain("company.departments[0].employees[0].email");
    }

    [Fact]
    public void ExtractJsonPaths_WithInvalidJson_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidJson = """{"user": {"name": "John"}"""; // Missing closing brace

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(invalidJson);

        // Assert
        paths.Should().BeEmpty();
    }

    [Fact]
    public void ExtractJsonPaths_WithEmptyObject_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyJson = "{}";

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(emptyJson);

        // Assert
        paths.Should().BeEmpty();
    }

    [Fact]
    public void ExtractJsonPaths_WithEmptyArray_ShouldReturnArrayPath()
    {
        // Arrange
        var jsonMessage = """{"items": []}""";

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(jsonMessage);

        // Assert
        paths.Should().Contain("items");
        paths.Should().HaveCount(1);
    }

    [Fact]
    public void ExtractJsonPaths_ShouldReturnSortedPaths()
    {
        // Arrange
        var jsonMessage = """{"z": "value", "a": "value", "m": "value"}""";

        // Act
        var paths = JsonReplacementService.ExtractJsonPaths(jsonMessage);

        // Assert
        paths.Should().BeInAscendingOrder();
        paths.Should().Equal("a", "m", "z");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ProcessMessage_WithNullOrEmptyMessage_ShouldReturnOriginalMessage(string message)
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "user.email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });

        // Act
        var result = _service.ProcessMessage(message, true);

        // Assert
        result.processedMessage.Should().Be(message);
        result.appliedRules.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMessage_WithDifferentDataTypes_ShouldReplaceAllTypes()
    {
        // Arrange
        _config.Rules.AddRange(new[]
        {
            new JsonReplacementRule { JsonPath = "stringValue", Placeholder = "[STRING]", Enabled = true },
            new JsonReplacementRule { JsonPath = "numberValue", Placeholder = "[NUMBER]", Enabled = true },
            new JsonReplacementRule { JsonPath = "boolValue", Placeholder = "[BOOL]", Enabled = true }
        });
        var originalMessage = """{"stringValue": "text", "numberValue": 42, "boolValue": true}""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement.GetProperty("stringValue").GetString().Should().Be("[STRING]");
        processedJson.RootElement.GetProperty("numberValue").GetString().Should().Be("[NUMBER]");
        processedJson.RootElement.GetProperty("boolValue").GetString().Should().Be("[BOOL]");
        result.appliedRules.Should().HaveCount(3);
    }

    [Fact]
    public void ProcessMessage_WithRootArrayAccess_ShouldReplaceRootArrayElement()
    {
        // Arrange
        _config.Rules.Add(new JsonReplacementRule 
        { 
            JsonPath = "[0].email", 
            Placeholder = "[EMAIL]", 
            Enabled = true 
        });
        var originalMessage = """[{"email": "first@example.com"}, {"email": "second@example.com"}]""";

        // Act
        var result = _service.ProcessMessage(originalMessage, true);

        // Assert
        var processedJson = JsonDocument.Parse(result.processedMessage);
        processedJson.RootElement[0].GetProperty("email").GetString().Should().Be("[EMAIL]");
        processedJson.RootElement[1].GetProperty("email").GetString().Should().Be("second@example.com");
        result.appliedRules.Should().Contain("[0].email → [EMAIL]");
    }

    [Fact]
    public void ProcessMessage_WithMultipleArrayElementsMatchingPath_ReplacesAllOccurrences()
    {
        // Arrange - Use exact specification from Bug 1.0.3 acceptance criteria
        var config = new JsonReplacementConfig
        {
            EnableReplacements = true,
            Rules = new List<JsonReplacementRule>
            {
                new JsonReplacementRule 
                { 
                    JsonPath = "person.employedAt", 
                    Placeholder = "{employed_at_id}", 
                    Enabled = true,
                    Description = "Replace employer ID with placeholder"
                }
            }
        };

        var service = new JsonReplacementService(config);
        // Use exact JSON structure from acceptance criteria
        var json = """[{"person": {"id": "123", "employedAt": "456"}}, {"person": {"id": "124", "employedAt": "456"}}]""";

        // Act
        var result = service.ProcessMessage(json, true);

        // Assert - Single rule should find and replace multiple occurrences
        result.appliedRules.Should().HaveCount(2, "because the single JsonPath rule should match both array elements");
        result.appliedRules.Should().Contain("person.employedAt → {employed_at_id}");
        
        // Verify both occurrences are replaced by parsing the result
        using var jsonDoc = JsonDocument.Parse(result.processedMessage);
        jsonDoc.RootElement[0].GetProperty("person").GetProperty("employedAt").GetString().Should().Be("{employed_at_id}");
        jsonDoc.RootElement[1].GetProperty("person").GetProperty("employedAt").GetString().Should().Be("{employed_at_id}");
    }
}
