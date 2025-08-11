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

## Bug 1.0.2: Console Output for PostUrls Configuration (2025-08-11)

**Feature**: Added console output to display PostUrls configuration for easy copy-paste

**Implementation**:
- Modified JMXGenerator to track newly added events during generation
- Added PrintPostUrlsConfiguration method to format JSON output
- Console output includes clear section headers and copy-paste ready JSON
- New events are marked with visual indicators (🆕 NEW)
- Added unit test to verify console output formatting

**User Experience**:
- Users can easily copy the complete PostUrls configuration
- New events are clearly identified with empty URLs
- Proper JSON formatting for direct paste into appsettings.json
- Visual indicators help users identify what needs configuration

**Code Pattern**:
```csharp
// Track newly added events
var newlyAddedEvents = new List<string>();

// During processing, track additions
if (urlMapping == null)
{
    postUrlsConfig.PostUrls.Add(new PostUrlMapping { EventName = eventName, Url = "" });
    newlyAddedEvents.Add(eventName);
}

// Print configuration at the end
PrintPostUrlsConfiguration(postUrlsConfig, newlyAddedEvents);
```

**Lessons Learned**:
- Console output formatting should be copy-paste ready for user convenience
- Visual indicators (emojis, markers) improve user experience significantly
- Tracking state changes during processing enables better user feedback
- Testing console output requires capturing stdout in unit tests 