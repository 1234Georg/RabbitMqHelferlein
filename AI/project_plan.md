# Project Plan

## Story Status Legend
- ⏳ **Pending** - Not started
- 🔄 **In Progress** - Currently being worked on  
- ✅ **Completed** - Implemented and verified
- 🧪 **Testing** - Implementation complete, awaiting verification

---

## Phase 1.0: Finish jMeter-Testfile-Generation

### Story 1.0.1: Set proper 'post-path' in jMeter-File
**Status:** ⏳ **Pending**

**As a developer, I want that each step in the generated jMeter-File uses the appropirate url to post data**

**Acceptance Criteria:**
- the event-name is part of the property from rabbitmq 'MT-MessageType', the event-name comes after the ':'
- the url replaces the value in the element 'stringProp' with the attribute 'name' and value 'HTTPSampler.path' in each test-step
- if there is an event which has no configuration for an url, the event is added to the configuration with an empty url

**Technical Implementation:**
- Configuration possibility in app-settings to map event-name to url

**Testing:**
- Manual: Panel resizing works smoothly
- Manual: Layout adapts correctly to different window sizes
- Manual: Panel collapse/expand maintains state
- Visual test: Layout feels professional and familiar

---

