# Technical Considerations & Decisions

## Overview
This file contains technical decisions, lessons learned, and implementation notes.

## Framework Upgrade - .NET 9 Migration (2025-08-11)

**Decision**: Upgraded from .NET 8 to .NET 9
**Rationale**: Keep framework current and benefit from latest performance improvements and language features
**Implementation**:
- Updated `<TargetFramework>` in both main and test projects
- Updated Microsoft.Extensions.* packages from 8.0.0 to 9.0.0  
- RabbitMQ.Client 7.0.0 remained compatible
- Build and all tests pass successfully

**Lessons Learned**:
- .NET 9 upgrade was seamless with no breaking changes
- Microsoft Extensions packages follow framework versioning
- Third-party libraries (RabbitMQ.Client) maintained compatibility

## Story 1.0.1: JMeter Template Event Naming (2025-08-11)

**Issue**: Tests were failing because event naming didn't match expected pattern when MT-MessageType header was missing

**Root Cause**: JMXGenerator used "Unknown" as fallback event name, but tests expected timestamp-based naming (Event_1_103045)

**Solution**: Implemented dual naming strategy:
1. Use MT-MessageType header for event name when available (after ':')
2. Fallback to timestamp-based naming: Event_{index}_{HHmmss} when header missing

**Code Pattern**:
```csharp
string testName;
if (!string.IsNullOrEmpty(eventName))
{
    testName = $"Event {eventName}";
}
else
{
    var timeString = eventData.Timestamp.ToString("HHmmss");
    testName = $"Event_{i + 1}_{timeString}";
}
```

**Lessons Learned**:
- Always check existing test expectations before modifying naming patterns
- Fallback strategies should be meaningful and testable
- Tests serve as specification - understand what they expect before changing behavior 