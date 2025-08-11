namespace RabbitMqEventConsumer;

public static class JMXGenerator
{
    public static void GenerateJMeterTemplate(List<ConsumedEvent> events, object eventsLock, PostUrlsConfig? postUrlsConfig = null)
    {
        Console.WriteLine();
        Console.WriteLine("🚀 JMeter Template Generator:");
        Console.WriteLine();
        
        var newlyAddedEvents = new List<string>();
        
        try
        {
            // Check if template files exist
            var templatePath = "JMeterTemplate.jmx";
            var teststepPath = "Teststep.jmx";
            
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"❌ Template file not found: {templatePath}");
                Console.WriteLine($"   Make sure JMeterTemplate.jmx exists in the application directory.");
                Console.WriteLine($"   {new string('─', 60)}");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(teststepPath))
            {
                Console.WriteLine($"❌ Teststep template file not found: {teststepPath}");
                Console.WriteLine($"   Make sure Teststep.jmx exists in the application directory.");
                Console.WriteLine($"   {new string('─', 60)}");
                Console.WriteLine();
                return;
            }

            var templateContent = File.ReadAllText(templatePath);
            var teststepTemplate = File.ReadAllText(teststepPath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var outputFileName = $"Generated_JMeter_Test_{timestamp}.jmx";
            
            Console.WriteLine($"📊 Processing captured events...");
            
            List<ConsumedEvent> eventsToProcess;
            lock (eventsLock)
            {
                eventsToProcess = events.ToList();
            }
            
            if (!eventsToProcess.Any())
            {
                Console.WriteLine($"⚠️  No captured events found. Using template without test steps.");
                // Just copy the template with empty test steps
                var emptyTestSteps = templateContent.Replace("<!--#Teststeps#-->", "");
                File.WriteAllText(outputFileName, emptyTestSteps);
            }
            else
            {
                Console.WriteLine($"   Found {eventsToProcess.Count} captured events");
                
                // Generate test steps from captured events
                var generatedTestSteps = GenerateTestStepsFromEvents(eventsToProcess, teststepTemplate, postUrlsConfig, newlyAddedEvents);
                
                // Replace the placeholder in the main template
                var finalContent = templateContent.Replace("<!--#Teststeps#-->", generatedTestSteps);
                File.WriteAllText(outputFileName, finalContent);
                
                Console.WriteLine($"   Generated {eventsToProcess.Count} test steps from captured events");
            }
            
            Console.WriteLine($"✅ JMeter template generated successfully!");
            Console.WriteLine($"   File: {outputFileName}");
            Console.WriteLine($"   Size: {new FileInfo(outputFileName).Length} bytes");
            Console.WriteLine();
            Console.WriteLine($"📝 Template Features:");
            Console.WriteLine($"   • OAuth authentication with Keycloak");
            Console.WriteLine($"   • Parameterized server configuration");
            Console.WriteLine($"   • Test data variables (FallId, PatientId, etc.)");
            Console.WriteLine($"   • HTTP samplers for API testing");
            Console.WriteLine($"   • JSON path assertions");
            Console.WriteLine($"   • Response validation");
            Console.WriteLine($"   • {eventsToProcess.Count} test steps generated from captured events");
            Console.WriteLine();
            Console.WriteLine($"🔧 Usage:");
            Console.WriteLine($"   1. Open {outputFileName} in Apache JMeter");
            Console.WriteLine($"   2. Configure the User Defined Variables:");
            Console.WriteLine($"      - Server: Target server hostname");
            Console.WriteLine($"      - Protocol: http or https");
            Console.WriteLine($"      - Port: Server port number");
            Console.WriteLine($"      - IdpServer: Keycloak server hostname");
            Console.WriteLine($"      - OAuthUsername/OAuthPassword: Test credentials");
            Console.WriteLine($"   3. Review and adjust the generated test steps");
            Console.WriteLine($"   4. Run the test plan");
            Console.WriteLine();
            Console.WriteLine($"💡 Tips:");
            Console.WriteLine($"   • Use JMeter variables like ${{FallId}} for dynamic data");
            Console.WriteLine($"   • Configure thread groups for load testing");
            Console.WriteLine($"   • Add listeners for result visualization");
            Console.WriteLine($"   • Use CSV Data Set Config for external test data");
            
            // Print PostUrls configuration if we have any events or newly added configurations
            if (postUrlsConfig != null && postUrlsConfig.PostUrls.Any())
            {
                PrintPostUrlsConfiguration(postUrlsConfig, newlyAddedEvents);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating JMeter template: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine($"   {new string('─', 60)}");
        Console.WriteLine();
    }

    private static string GenerateTestStepsFromEvents(List<ConsumedEvent> events, string teststepTemplate, PostUrlsConfig? postUrlsConfig, List<string> newlyAddedEvents)
    {
        var testSteps = new List<string>();
        
        for (int i = 0; i < events.Count; i++)
        {
            var eventData = events[i];
            
            try
            {
                // Determine which message to use (processed if available, otherwise original)
                var messageToUse = eventData.HasReplacements && !string.IsNullOrEmpty(eventData.ProcessedMessage)
                    ? eventData.ProcessedMessage
                    : eventData.Message;
                
                // XML escape the message content
                var xmlEscapedPayload = XmlEscapeString(messageToUse);
                
                // Replace the payload placeholder in the test step template
                var testStepContent = teststepTemplate.Replace("<!--#payload#-->", xmlEscapedPayload);

                // Extract event name from MT-MessageType header
                string? eventName = null;
                string? fullMessageType = null;
                if (eventData.Headers.TryGetValue("MT-MessageType", out fullMessageType) ||
                    eventData.Headers.TryGetValue("MT-Fault-MessageType", out fullMessageType))
                {
                    // Extract event name after the ':'
                    var colonIndex = fullMessageType.LastIndexOf(':');
                    eventName = colonIndex >= 0 ? fullMessageType.Substring(colonIndex + 1) : fullMessageType;
                }

                // Look up URL for this event name
                string? eventUrl = null;
                if (!string.IsNullOrEmpty(eventName) && postUrlsConfig?.PostUrls != null)
                {
                    var urlMapping = postUrlsConfig.PostUrls.FirstOrDefault(u => u.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase));
                    eventUrl = urlMapping?.Url;
                    
                    // If no configuration found, add it to the list with empty URL
                    if (urlMapping == null)
                    {
                        postUrlsConfig.PostUrls.Add(new PostUrlMapping { EventName = eventName, Url = "" });
                        newlyAddedEvents.Add(eventName);
                        Console.WriteLine($"⚠️  Added new event '{eventName}' to configuration with empty URL");
                    }
                }

                // Replace HTTPSampler.path if URL is configured
                if (!string.IsNullOrEmpty(eventUrl) && testStepContent.Contains("name=\"HTTPSampler.path\""))
                {
                    // Find and replace the HTTPSampler.path value
                    var pathPattern = @"<stringProp name=""HTTPSampler\.path"">[^<]*</stringProp>";
                    var replacement = $"<stringProp name=\"HTTPSampler.path\">{eventUrl}</stringProp>";
                    testStepContent = System.Text.RegularExpressions.Regex.Replace(testStepContent, pathPattern, replacement);
                }

                // Customize the test name with event information
                string testName;
                if (!string.IsNullOrEmpty(eventName))
                {
                    testName = $"Event {eventName}";
                }
                else
                {
                    // Use timestamp-based naming as fallback
                    var timeString = eventData.Timestamp.ToString("HHmmss");
                    testName = $"Event_{i + 1}_{timeString}";
                }
                testStepContent = testStepContent.Replace("[mtec] Patient anlegen", $"[Generated] {testName}");
                
                testSteps.Add(testStepContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Warning: Could not process event {i + 1}: {ex.Message}");
                // Continue with other events
            }
        }
        
        return string.Join("\n", testSteps);
    }

    private static void PrintPostUrlsConfiguration(PostUrlsConfig postUrlsConfig, List<string> newlyAddedEvents)
    {
        Console.WriteLine();
        Console.WriteLine("📋 PostUrls Configuration:");
        Console.WriteLine();
        
        if (newlyAddedEvents.Any())
        {
            Console.WriteLine($"   🆕 {newlyAddedEvents.Count} new event(s) were added with empty URLs:");
            foreach (var eventName in newlyAddedEvents)
            {
                Console.WriteLine($"      • {eventName}");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine("   📄 Copy this configuration to your appsettings.json:");
        Console.WriteLine();
        Console.WriteLine("   \"PostUrls\": [");
        
        for (int i = 0; i < postUrlsConfig.PostUrls.Count; i++)
        {
            var mapping = postUrlsConfig.PostUrls[i];
            var isNewlyAdded = newlyAddedEvents.Contains(mapping.EventName);
            var marker = isNewlyAdded ? " // 🆕 NEW - Please configure URL" : "";
            var comma = i < postUrlsConfig.PostUrls.Count - 1 ? "," : "";
            
            Console.WriteLine($"     {{");
            Console.WriteLine($"       \"EventName\": \"{mapping.EventName}\",");
            Console.WriteLine($"       \"Url\": \"{mapping.Url}\"{marker}");
            Console.WriteLine($"     }}{comma}");
        }
        
        Console.WriteLine("   ]");
        Console.WriteLine();
        
        if (newlyAddedEvents.Any())
        {
            Console.WriteLine($"   💡 Please update the empty URLs for the newly added events in your appsettings.json");
        }
    }

    private static string XmlEscapeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        return input
            .Replace("&", "&amp;")   // Must be first
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
