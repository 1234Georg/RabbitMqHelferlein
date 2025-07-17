# JMX File Generation Tests Documentation

## Overview

This directory contains comprehensive test coverage for the enhanced JMX file generation feature. The tests verify that the system correctly generates Apache JMeter test plans from captured RabbitMQ events with proper XML escaping and template processing.

## Test Structure

### Test Files

#### `JMXGeneratorBasicTests.cs`
**Purpose**: Core xunit tests for basic JMX generation functionality

**Test Coverage**:
- ✅ Empty event list handling
- ✅ Single event processing 
- ✅ JSON replacement integration
- ✅ XML character escaping
- ✅ Multiple event processing

#### `TestEventFactory.cs`
**Purpose**: Factory class for creating standardized test events

**Provides**:
- Patient registration events (with JSON replacements)
- Appointment scheduling events (without replacements)
- Billing events (complex nested JSON)
- System alert events (non-JSON content)
- Special character events (XML escaping tests)
- Failed event processing scenarios
- Large payload performance tests
- Workflow sequence events

## Test Categories

### 1. **Basic Functionality Tests**

```csharp
[Fact]
JMXGenerator_WithNoEvents_GeneratesValidTemplate()
```
- **Validates**: Empty event list creates valid XML template
- **Checks**: Template placeholders are properly replaced
- **Verifies**: Generated XML is well-formed

```csharp
[Fact]  
JMXGenerator_WithSingleEvent_CreatesTestStep()
```
- **Validates**: Single event generates one test step
- **Checks**: Event data is properly embedded
- **Verifies**: Test step naming and content

### 2. **JSON Replacement Integration Tests**

```csharp
[Fact]
JMXGenerator_WithProcessedEvent_UsesProcessedMessage()
```
- **Validates**: Processed messages take priority over original
- **Checks**: Placeholder variables are preserved in output
- **Verifies**: Original values are not present

### 3. **XML Escaping Tests**

```csharp
[Fact]
JMXGenerator_WithSpecialCharacters_EscapesCorrectly()
```
- **Validates**: All XML special characters are properly escaped
- **Checks**: `&`, `<`, `>`, `"`, `'` → `&amp;`, `&lt;`, `&gt;`, `&quot;`, `&apos;`
- **Verifies**: Resulting XML remains valid after escaping

### 4. **Multi-Event Processing Tests**

```csharp
[Fact]
JMXGenerator_WithMultipleEvents_CreatesMultipleTestSteps()
```
- **Validates**: Multiple events generate multiple test steps
- **Checks**: Test steps are created in correct sequence
- **Verifies**: Each event contributes exactly one test step

## Test Event Types

### Healthcare Domain Events

#### Patient Registration Event
```json
{
  "patientId": "P12345",
  "firstName": "Max", 
  "lastName": "Mustermann",
  "birthDate": "1985-03-15",
  "email": "max.mustermann@example.com"
}
```
**With Replacements**:
- `patientId` → `${PatientId}`
- `firstName` → `${FirstName}`
- etc.

#### Appointment Event
```json
{
  "appointmentId": "APT67890",
  "patientId": "P12345",
  "doctorId": "DOC456",
  "dateTime": "2024-01-16T10:00:00Z",
  "type": "consultation"
}
```
**No Replacements**: Used to test original content handling

#### Billing Event
```json
{
  "invoice": {
    "id": "INV-2024-001",
    "amount": 285.50,
    "items": [...]
  }
}
```
**Complex JSON**: Tests nested object handling

### Special Test Cases

#### XML Character Event
```json
{
  "message": "Testing special chars: <>&\"'",
  "description": "Dr. Smith & Associates said \"Patient's condition is <stable>\""
}
```
**Purpose**: Validates XML escaping works correctly

#### System Alert (Non-JSON)
```
SYSTEM_ALERT: Database backup completed successfully. Status: SUCCESS
```
**Purpose**: Tests non-JSON content handling

## Test Infrastructure

### Helper Methods

#### Template Creation
```csharp
CreateTestTemplates()
```
- Creates minimal but valid JMeter templates
- Sets up `<!--#Teststeps#-->` and `<!--#payload#-->` placeholders
- Ensures proper XML structure

#### XML Validation
```csharp
XmlDocument.LoadXml(content)
```
- Validates generated content is well-formed XML
- Catches XML parsing errors
- Ensures template processing doesn't break structure

#### Content Verification
```csharp
CountOccurrences(string text, string pattern)
```
- Counts specific patterns in generated content
- Validates expected number of test steps
- Checks placeholder replacement

## Test Environment

### Isolation
- Each test creates unique temporary directory
- Templates created fresh for each test
- No cross-test contamination

### Cleanup
- `IDisposable` pattern ensures cleanup
- Temporary files removed after each test
- No file system pollution

### Current Directory Management
- Tests set working directory to temp location
- Ensures template files are found correctly
- Isolates from actual project files

## Assertions Validated

### Content Structure
- ✅ Valid XML structure maintained
- ✅ JMeter test plan elements present
- ✅ HTTP sampler components included
- ✅ Proper XML namespaces and attributes

### Placeholder Replacement
- ✅ `<!--#Teststeps#-->` replaced with actual test steps
- ✅ `<!--#payload#-->` replaced with event content
- ✅ No template placeholders remain in output

### Data Processing
- ✅ JSON replacements preserved in output
- ✅ XML escaping applied correctly
- ✅ Original data not leaked when processed
- ✅ Event metadata reflected in test names

### File Generation
- ✅ Output file created with timestamped name
- ✅ File size appropriate for content
- ✅ File encoding correct (UTF-8)

## Running Tests

### Command Line
```bash
cd RabbitMqEventConsumer.Tests
dotnet test
```
