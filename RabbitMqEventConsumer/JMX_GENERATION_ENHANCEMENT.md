# Enhanced JMX File Generation Feature

## Overview

The JMX file generation feature has been significantly extended to automatically create JMeter test plans from captured RabbitMQ events. This enhancement allows for automatic generation of HTTP test steps based on real event data.

## Key Features

### 1. Event-Driven Test Generation
- **Automatic Test Step Creation**: Each captured event becomes a separate HTTP test step
- **Payload Integration**: Event messages (original or processed) are XML-escaped and embedded as request payloads
- **Dynamic Test Names**: Test steps are automatically named with event sequence and timestamp

### 2. Template-Based Architecture
- **Main Template**: Uses `JMeterTemplate.jmx` as the base structure
- **Test Step Template**: Uses `Teststep.jmx` for individual HTTP request steps
- **Placeholder Replacement**:
  - `<!--#payload#-->` in `Teststep.jmx` is replaced with XML-escaped event content
  - `<!--#Teststeps#-->` in `JMeterTemplate.jmx` is replaced with all generated test steps

### 3. Smart Payload Selection
- **Processed Messages First**: If JSON replacements were applied, uses the processed message
- **Fallback to Original**: If no processing occurred, uses the original event message
- **XML Escaping**: All content is properly XML-escaped to ensure valid JMX structure

## Implementation Details

### File Structure
```
RabbitMqEventConsumer/
├── JMeterTemplate.jmx          # Main JMeter test plan template
├── Teststep.jmx                # Individual HTTP request template
├── JMXGenerator.cs             # Enhanced generation logic
└── Program.cs                  # Updated to use new generator
```

### Code Architecture

#### JMXGenerator.cs
```csharp
public static class JMXGenerator
{
    // Main generation method
    public static void GenerateJMeterTemplate(List<ConsumedEvent> events, object eventsLock)
    
    // Creates test steps from events
    private static string GenerateTestStepsFromEvents(List<ConsumedEvent> events, string teststepTemplate)
    
    // XML escaping utility
    private static string XmlEscapeString(string input)
}
```

#### Key Methods

**GenerateTestStepsFromEvents**
- Iterates through captured events
- Selects appropriate message content (processed vs. original)
- Applies XML escaping to payload
- Replaces template placeholders
- Generates unique test step names

**XmlEscapeString**
- Escapes XML special characters: `&`, `<`, `>`, `"`, `'`
- Ensures proper XML compliance
- Prevents parsing errors in generated JMX files

## Usage Instructions

### 1. Capturing Events
1. Run the RabbitMQ Event Consumer
2. Let it capture events from your message queue
3. Events are automatically stored in memory with their metadata

### 2. Generating JMX Files
1. Press **[J]** in the console application
2. The system will:
   - Check for required template files
   - Process all captured events
   - Generate a timestamped JMX file

### 3. Using Generated Files
1. Open the generated `.jmx` file in Apache JMeter
2. Configure server variables in the "User Defined Variables" section
3. Review and adjust the generated test steps as needed
4. Run your performance tests

## Template Requirements

### JMeterTemplate.jmx
Must contain the placeholder `<!--#Teststeps#-->` where generated test steps will be inserted.

### Teststep.jmx
Must contain the placeholder `<!--#payload#-->` where event content will be inserted.

## Example Generated Content

### Input Event
```json
{"patientId": "12345", "name": "Max Mustermann", "birthDate": "1985-03-15"}
```

### Generated Test Step
```xml
<Teststep>
    <HTTPSamplerProxy guiclass="HttpTestSampleGui" testclass="HTTPSamplerProxy" testname="[Generated] Event_1_134642" enabled="true">
        <!-- ... configuration ... -->
        <stringProp name="Argument.value">{&quot;patientId&quot;: &quot;12345&quot;, &quot;name&quot;: &quot;Max Mustermann&quot;, &quot;birthDate&quot;: &quot;1985-03-15&quot;}
</stringProp>
        <!-- ... -->
    </HTTPSamplerProxy>
    <!-- ... headers, assertions ... -->
</Teststep>
```

## Benefits

1. **Automated Test Creation**: No manual creation of test steps
2. **Real Data Integration**: Uses actual production/test data
3. **JSON Replacement Support**: Leverages existing parameterization features
4. **Scalable**: Handles any number of captured events
5. **Customizable**: Template-based approach allows easy modifications

## Error Handling

- **Missing Templates**: Clear error messages if template files are not found
- **Event Processing Errors**: Individual event failures don't stop the entire generation
- **Empty Event History**: Gracefully handles cases with no captured events
- **XML Escaping**: Ensures all special characters are properly escaped

## Future Enhancements

Potential improvements could include:
- Custom endpoint URL mapping
- Advanced payload transformation
- Test data correlation
- Performance test pattern recognition
- Dynamic assertion generation based on response patterns
