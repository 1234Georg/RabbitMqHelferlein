using System.Xml;
using RabbitMqEventConsumer;

namespace RabbitMqEventConsumer.Tests;

/// <summary>
/// Simple xunit tests for JMX generation functionality
/// </summary>
public class JMXGeneratorBasicTests : IDisposable
{
    private readonly string _testOutputDirectory;

    public JMXGeneratorBasicTests()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "JMXBasicTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testOutputDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void JMXGenerator_WithNoEvents_GeneratesValidTemplate()
    {
        // Arrange
        var events = new List<ConsumedEvent>();
        var lockObject = new object();
        CreateTestTemplates();
        SetCurrentDirectory();

        // Act
        JMXGenerator.GenerateJMeterTemplate(events, lockObject);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDirectory, "Generated_JMeter_Test_*.jmx");
        Assert.Single(generatedFiles);

        var content = File.ReadAllText(generatedFiles[0]);
        Assert.DoesNotContain("<!--#Teststeps#-->", content);
        Assert.Contains("<jmeterTestPlan", content);

        // Verify it's valid XML
        var doc = new XmlDocument();
        var exception = Record.Exception(() => doc.LoadXml(content));
        Assert.Null(exception);
    }

    [Fact]
    public void JMXGenerator_WithSingleEvent_CreatesTestStep()
    {
        // Arrange
        var events = new List<ConsumedEvent>
        {
            new ConsumedEvent
            {
                Timestamp = new DateTime(2024, 1, 15, 10, 30, 45),
                Message = """{"userId": "12345", "action": "login"}""",
                HasReplacements = false,
                IsJson = true,
                MessageSize = 42
            }
        };
        var lockObject = new object();
        CreateTestTemplates();
        SetCurrentDirectory();

        // Act
        JMXGenerator.GenerateJMeterTemplate(events, lockObject);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDirectory, "Generated_JMeter_Test_*.jmx");
        var content = File.ReadAllText(generatedFiles[0]);

        // Verify specific content
        Assert.Contains("[Generated] Event_1_103045", content);
        Assert.Contains("userId", content);
        Assert.Contains("&quot;", content); // XML escaping
        Assert.DoesNotContain("<!--#payload#-->", content);

        // Verify it's valid XML
        var doc = new XmlDocument();
        var exception = Record.Exception(() => doc.LoadXml(content));
        Assert.Null(exception);
    }

    [Fact]
    public void JMXGenerator_WithProcessedEvent_UsesProcessedMessage()
    {
        // Arrange
        var events = new List<ConsumedEvent>
        {
            new ConsumedEvent
            {
                Timestamp = new DateTime(2024, 1, 15, 14, 22, 30),
                Message = """{"userId": "12345", "sessionId": "abc123"}""",
                ProcessedMessage = """{"userId": "${UserId}", "sessionId": "${SessionId}"}""",
                HasReplacements = true,
                IsJson = true,
                MessageSize = 50
            }
        };
        var lockObject = new object();
        CreateTestTemplates();
        SetCurrentDirectory();

        // Act
        JMXGenerator.GenerateJMeterTemplate(events, lockObject);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDirectory, "Generated_JMeter_Test_*.jmx");
        var content = File.ReadAllText(generatedFiles[0]);

        // Should use processed message, not original
        Assert.Contains("${UserId}", content);
        Assert.Contains("${SessionId}", content);
        Assert.DoesNotContain("12345", content);
        Assert.DoesNotContain("abc123", content);
    }

    [Fact]
    public void JMXGenerator_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var events = new List<ConsumedEvent>
        {
            new ConsumedEvent
            {
                Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
                Message = """{"message": "Hello & welcome to <Company>!", "quote": "It's \"great\""}""",
                HasReplacements = false,
                IsJson = true,
                MessageSize = 75
            }
        };
        var lockObject = new object();
        CreateTestTemplates();
        SetCurrentDirectory();

        // Act
        JMXGenerator.GenerateJMeterTemplate(events, lockObject);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDirectory, "Generated_JMeter_Test_*.jmx");
        var content = File.ReadAllText(generatedFiles[0]);

        // Verify XML escaping
        Assert.Contains("&amp;", content);
        Assert.Contains("&lt;", content);
        Assert.Contains("&gt;", content);
        Assert.Contains("&quot;", content);

        // Verify it's still valid XML after escaping
        var doc = new XmlDocument();
        var exception = Record.Exception(() => doc.LoadXml(content));
        Assert.Null(exception);
    }

    [Fact]
    public void JMXGenerator_WithMultipleEvents_CreatesMultipleTestSteps()
    {
        // Arrange
        var events = new List<ConsumedEvent>
        {
            TestEventFactory.CreatePatientRegistrationEvent(new DateTime(2024, 1, 15, 9, 15, 0)),
            TestEventFactory.CreateAppointmentEvent(new DateTime(2024, 1, 15, 9, 16, 30)),
            TestEventFactory.CreateBillingEvent(new DateTime(2024, 1, 15, 9, 17, 45))
        };
        var lockObject = new object();
        CreateTestTemplates();
        SetCurrentDirectory();

        // Act
        JMXGenerator.GenerateJMeterTemplate(events, lockObject);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDirectory, "Generated_JMeter_Test_*.jmx");
        var content = File.ReadAllText(generatedFiles[0]);

        // Verify all test steps are present
        Assert.Contains("[Generated] Event_1_091500", content);
        Assert.Contains("[Generated] Event_2_091630", content);
        Assert.Contains("[Generated] Event_3_091745", content);

        // Count test step occurrences
        var testStepCount = CountOccurrences(content, "<Teststep>");
        Assert.Equal(3, testStepCount);
    }

    #region Helper Methods

    private void CreateTestTemplates()
    {
        CreateMainTemplate();
        CreateTestStepTemplate();
    }

    private void CreateMainTemplate()
    {
        var mainTemplate = """
            <?xml version="1.0" encoding="UTF-8"?>
            <jmeterTestPlan version="1.2" properties="5.0" jmeter="5.6.3">
              <hashTree>
                <TestPlan guiclass="TestPlanGui" testclass="TestPlan" testname="Generated Test Plan">
                  <boolProp name="TestPlan.functional_mode">false</boolProp>
                </TestPlan>
                <hashTree>
                  <ThreadGroup guiclass="ThreadGroupGui" testclass="ThreadGroup" testname="Main Thread Group">
                    <intProp name="ThreadGroup.num_threads">1</intProp>
                    <intProp name="ThreadGroup.ramp_time">1</intProp>
                  </ThreadGroup>
                  <hashTree>
                    <!--#Teststeps#-->
                  </hashTree>
                </hashTree>
              </hashTree>
            </jmeterTestPlan>
            """;
            
        File.WriteAllText(Path.Combine(_testOutputDirectory, "JMeterTemplate.jmx"), mainTemplate);
    }

    private void CreateTestStepTemplate()
    {
        var stepTemplate = """
            <Teststep>
              <HTTPSamplerProxy guiclass="HttpTestSampleGui" testclass="HTTPSamplerProxy" testname="[mtec] Patient anlegen" enabled="true">
                <stringProp name="HTTPSampler.domain">${Server}</stringProp>
                <stringProp name="HTTPSampler.method">POST</stringProp>
                <boolProp name="HTTPSampler.postBodyRaw">true</boolProp>
                <elementProp name="HTTPsampler.Arguments" elementType="Arguments">
                  <collectionProp name="Arguments.arguments">
                    <elementProp name="" elementType="HTTPArgument">
                      <stringProp name="Argument.value"><!--#payload#--></stringProp>
                    </elementProp>
                  </collectionProp>
                </elementProp>
              </HTTPSamplerProxy>
            </Teststep>
            """;
            
        File.WriteAllText(Path.Combine(_testOutputDirectory, "Teststep.jmx"), stepTemplate);
    }

    private void SetCurrentDirectory()
    {
        Directory.SetCurrentDirectory(_testOutputDirectory);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    #endregion
}
