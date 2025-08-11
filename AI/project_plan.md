# Project Plan

## Story Status Legend
- ⏳ **Pending** - Not started
- 🔄 **In Progress** - Currently being worked on  
- ✅ **Completed** - Implemented and verified
- 🧪 **Testing** - Implementation complete, awaiting verification

---

## Phase 1.0: Finish jMeter-Testfile-Generation

### Story 1.0.1: Set proper 'post-path' in jMeter-File
**Status:** ✅ **Completed**

**As a developer, I want that each step in the generated jMeter-File uses the appropirate url to post data**

**Acceptance Criteria:**
- the event-name is part of the property from rabbitmq 'MT-MessageType', the event-name comes after the ':'
- the url replaces the value in the element 'stringProp' with the attribute 'name' and value 'HTTPSampler.path' in each test-step
- if there is an event which has no configuration for an url, the event is added to the configuration with an empty url

**Technical Implementation:**
- Configuration possibility in app-settings to map event-name to url

**Testing:**
- Unittest: replacement of url
- Unittest: missing configuration and no replacement but adding empty config entry

---

### Bug 1.0.2: Print the new generated configuration for the 'post-path' (see Story 1.0.1) to console, so that the user can copy it directly to the configuration file.
**Status:** ✅ **Completed**

**Acceptance Criteria:**
- log-output on console with the proper format implemented in Story 1.0.1, so that the user can copy it directly to it's app-settings file

---

### Bug 1.0.3: Add a unitest to verify that multiple json-path replacements happen corretly
**Status:** ✅ **Completed**

**Acceptance Criteria:**
- Unittest that 

example event
```
{
    [
        "person": {
            "id": "123",
            "employedAt": "456"
        },
        "person": {
            "id": "124",
            "employedAt": "456"
        }
    ]
}
```
use the following replacement rules
```
"JsonReplacement": {
    "Rules": [
      {
        "JsonPath": "person.empoyedAt",
        "Placeholder": "{employed_at_id}",
        "Enabled": true,
        "Description": "Replace employer ID with placeholder"
      }
```
The expected value after replacement is
```
{
    [
        "person": {
            "id": "123",
            "employedAt": "{employed_at_id}"
        },
        "person": {
            "id": "124",
            "employedAt": "{employed_at_id}"
        }
    ]
}
```
---

### Bug 1.0.3: Fix the warnings
**Status:** ⏳ **Pending**

**Acceptance Criteria:**
- all warnings are fixed
- there is a build error when there is a warning

**Hints**
- Ask human when not clear how to fix it

---