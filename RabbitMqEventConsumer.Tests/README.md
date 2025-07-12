# JsonReplacementService Unit Tests

This directory contains comprehensive unit tests for the `JsonReplacementService` class and related configuration classes.

## Test Coverage

### JsonReplacementServiceTests

The test suite covers the following functionality:

#### Core Functionality
- **Constructor initialization** - Ensures the service properly initializes with configuration
- **Message processing** - Tests the main `ProcessMessage` method under various conditions
- **JSON path extraction** - Tests the `ExtractJsonPaths` static method

#### Configuration Scenarios
- ✅ Replacements disabled - Should return original message
- ✅ Non-JSON content - Should return original message  
- ✅ No enabled rules - Should return original message
- ✅ Empty/whitespace messages - Should handle gracefully

#### JSON Path Support
- ✅ Simple object properties (`user.email`)
- ✅ Nested object properties (`data.user.profile.personal.email`)  
- ✅ Array access (`users[0].email`)
- ✅ Direct array element access (`emails[1]`)
- ✅ Root array access (`[0].email`)
- ✅ Complex nested structures with arrays and objects

#### Rule Processing
- ✅ Multiple enabled rules - Should apply all
- ✅ Mixed enabled/disabled rules - Should only apply enabled ones
- ✅ Invalid JSON paths - Should skip gracefully
- ✅ Invalid array indices - Should skip gracefully
- ✅ Different data types (string, number, boolean) - Should replace all

#### Error Handling
- ✅ Invalid JSON - Should return original message
- ✅ Malformed JSON paths - Should skip rule
- ✅ Array index out of bounds - Should skip rule
- ✅ Missing properties - Should skip rule

#### Path Extraction
- ✅ Simple objects - Extract all property paths
- ✅ Arrays - Extract array index paths
- ✅ Complex nested structures - Extract all nested paths
- ✅ Empty objects/arrays - Handle appropriately
- ✅ Invalid JSON - Return empty list
- ✅ Path sorting - Results are properly sorted

### JsonReplacementConfigTests

Tests the configuration classes:

#### JsonReplacementConfig
- ✅ Default values validation
- ✅ Property assignment
- ✅ Rules collection management

#### JsonReplacementRule  
- ✅ Default values validation
- ✅ Property assignment
- ✅ Different configuration scenarios

## Test Statistics

- **Total Tests**: 30
- **Test Classes**: 2  
- **Test Methods**: JsonReplacementServiceTests (27), JsonReplacementConfigTests (3)
- **Coverage**: Comprehensive coverage of all public methods and edge cases

## Running the Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "ClassName=JsonReplacementServiceTests"
```

## Test Framework

- **Testing Framework**: xUnit
- **Assertion Library**: FluentAssertions  
- **Target Framework**: .NET 9.0

## Test Data Patterns

The tests use various JSON patterns to ensure comprehensive coverage:

- Simple objects with primitive values
- Nested object hierarchies  
- Arrays with objects
- Mixed data types (strings, numbers, booleans)
- Complex real-world-like structures (company/department/employee)
- Edge cases (empty objects, arrays, invalid JSON)

All tests follow the Arrange-Act-Assert pattern and include descriptive names that clearly indicate the scenario being tested.
