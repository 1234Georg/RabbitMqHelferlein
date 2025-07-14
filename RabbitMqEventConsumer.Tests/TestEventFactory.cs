namespace RabbitMqEventConsumer.Tests;

/// <summary>
/// Factory class for creating standardized test events for JMX generation testing
/// </summary>
public static class TestEventFactory
{
    /// <summary>
    /// Creates a basic patient registration event with JSON replacements
    /// </summary>
    public static ConsumedEvent CreatePatientRegistrationEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now,
            Exchange = "hospital.integration",
            RoutingKey = "patient.registration",
            DeliveryTag = 1001,
            Message = """{"patientId": "P12345", "firstName": "Max", "lastName": "Mustermann", "birthDate": "1985-03-15", "email": "max.mustermann@example.com"}""",
            ProcessedMessage = """{"patientId": "${PatientId}", "firstName": "${FirstName}", "lastName": "${LastName}", "birthDate": "${BirthDate}", "email": "${PatientEmail}"}""",
            HasReplacements = true,
            IsJson = true,
            MessageSize = 134,
            ContentType = "application/json",
            MessageId = "msg-patient-001",
            CorrelationId = "corr-registration-001",
            ProcessedSuccessfully = true,
            AppliedReplacements = new List<string>
            {
                "PatientId: P12345 -> ${PatientId}",
                "FirstName: Max -> ${FirstName}",
                "LastName: Mustermann -> ${LastName}",
                "BirthDate: 1985-03-15 -> ${BirthDate}",
                "Email: max.mustermann@example.com -> ${PatientEmail}"
            },
            Headers = new Dictionary<string, string>
            {
                { "source", "registration-service" },
                { "version", "1.0" },
                { "timestamp", timestamp?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            }
        };
    }

    /// <summary>
    /// Creates an appointment scheduling event without JSON replacements
    /// </summary>
    public static ConsumedEvent CreateAppointmentEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(2),
            Exchange = "hospital.scheduling",
            RoutingKey = "appointment.scheduled",
            DeliveryTag = 1002,
            Message = """{"appointmentId": "APT67890", "patientId": "P12345", "doctorId": "DOC456", "dateTime": "2024-01-16T10:00:00Z", "type": "consultation", "duration": 30}""",
            HasReplacements = false,
            IsJson = true,
            MessageSize = 156,
            ContentType = "application/json",
            MessageId = "msg-appointment-001",
            CorrelationId = "corr-appointment-001",
            ProcessedSuccessfully = true,
            Headers = new Dictionary<string, string>
            {
                { "source", "scheduling-service" },
                { "version", "2.1" },
                { "priority", "normal" }
            }
        };
    }

    /// <summary>
    /// Creates a billing event with complex nested JSON
    /// </summary>
    public static ConsumedEvent CreateBillingEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(5),
            Exchange = "billing.events",
            RoutingKey = "invoice.generated",
            DeliveryTag = 1003,
            Message = """{"invoice": {"id": "INV-2024-001", "patientId": "P12345", "amount": 285.50, "currency": "EUR", "status": "pending", "items": [{"code": "CONS", "description": "Medical Consultation", "quantity": 1, "unitPrice": 150.00}, {"code": "LAB", "description": "Laboratory Tests", "quantity": 3, "unitPrice": 45.17}], "dueDate": "2024-02-15"}}""",
            ProcessedMessage = """{"invoice": {"id": "${InvoiceId}", "patientId": "${PatientId}", "amount": "${InvoiceAmount}", "currency": "${Currency}", "status": "${InvoiceStatus}", "items": [{"code": "${ItemCode1}", "description": "${ItemDesc1}", "quantity": "${ItemQty1}", "unitPrice": "${ItemPrice1}"}, {"code": "${ItemCode2}", "description": "${ItemDesc2}", "quantity": "${ItemQty2}", "unitPrice": "${ItemPrice2}"}], "dueDate": "${DueDate}"}}""",
            HasReplacements = true,
            IsJson = true,
            MessageSize = 398,
            ContentType = "application/json",
            MessageId = "msg-billing-001",
            CorrelationId = "corr-billing-001",
            ProcessedSuccessfully = true,
            AppliedReplacements = new List<string>
            {
                "InvoiceId: INV-2024-001 -> ${InvoiceId}",
                "PatientId: P12345 -> ${PatientId}",
                "Amount: 285.50 -> ${InvoiceAmount}",
                "Status: pending -> ${InvoiceStatus}"
            }
        };
    }

    /// <summary>
    /// Creates a system alert event (non-JSON)
    /// </summary>
    public static ConsumedEvent CreateSystemAlertEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(8),
            Exchange = "system.alerts",
            RoutingKey = "database.backup",
            DeliveryTag = 1004,
            Message = "SYSTEM_ALERT: Database backup completed successfully. Backup size: 2.5GB, Duration: 45min, Status: SUCCESS",
            HasReplacements = false,
            IsJson = false,
            MessageSize = 115,
            ContentType = "text/plain",
            MessageId = "msg-system-001",
            ProcessedSuccessfully = true,
            Headers = new Dictionary<string, string>
            {
                { "source", "backup-service" },
                { "severity", "info" },
                { "category", "maintenance" }
            }
        };
    }

    /// <summary>
    /// Creates an event with special XML characters for escaping tests
    /// </summary>
    public static ConsumedEvent CreateSpecialCharactersEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(10),
            Exchange = "test.messages",
            RoutingKey = "special.characters",
            DeliveryTag = 1005,
            Message = """{"message": "Testing special chars: <>&\"'", "description": "Dr. Smith & Associates said \"Patient's condition is <stable>\"", "notes": "It's a 'complex' case"}""",
            ProcessedMessage = """{"message": "${TestMessage}", "description": "${DoctorNote}", "notes": "${CaseNotes}"}""",
            HasReplacements = true,
            IsJson = true,
            MessageSize = 178,
            ContentType = "application/json",
            MessageId = "msg-special-001",
            ProcessedSuccessfully = true,
            AppliedReplacements = new List<string>
            {
                "TestMessage: Testing special chars: <>&\"' -> ${TestMessage}",
                "DoctorNote: Dr. Smith & Associates said \"Patient's condition is <stable>\" -> ${DoctorNote}",
                "CaseNotes: It's a 'complex' case -> ${CaseNotes}"
            }
        };
    }

    /// <summary>
    /// Creates an event that failed processing
    /// </summary>
    public static ConsumedEvent CreateFailedEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(12),
            Exchange = "error.events",
            RoutingKey = "processing.failed",
            DeliveryTag = 1006,
            Message = """{"invalidJson": "missing quote}""",
            HasReplacements = false,
            IsJson = false, // Marked as false due to invalid JSON
            MessageSize = 31,
            ContentType = "application/json",
            MessageId = "msg-failed-001",
            ProcessedSuccessfully = false,
            ErrorMessage = "Invalid JSON format: Unexpected character encountered while parsing value"
        };
    }

    /// <summary>
    /// Creates a minimal event with only required fields
    /// </summary>
    public static ConsumedEvent CreateMinimalEvent(DateTime? timestamp = null)
    {
        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(15),
            Exchange = "minimal.test",
            RoutingKey = "basic",
            DeliveryTag = 1007,
            Message = "{}",
            HasReplacements = false,
            IsJson = true,
            MessageSize = 2,
            ProcessedSuccessfully = true
        };
    }

    /// <summary>
    /// Creates a large event for performance testing
    /// </summary>
    public static ConsumedEvent CreateLargeEvent(DateTime? timestamp = null, int sizeMultiplier = 10)
    {
        var baseData = "{\"patientId\": \"P12345\", \"data\": ";
        var largeData = string.Join(",", Enumerable.Range(0, sizeMultiplier * 100).Select(i => $"\"field{i}\": \"value{i}\""));
        var largeMessage = baseData + largeData + "\"}";

        return new ConsumedEvent
        {
            Timestamp = timestamp ?? DateTime.Now.AddMinutes(20),
            Exchange = "performance.test",
            RoutingKey = "large.payload",
            DeliveryTag = 1008,
            Message = largeMessage,
            ProcessedMessage = """{"patientId": "${PatientId}", "data": "${LargeDataSet}"}""",
            HasReplacements = true,
            IsJson = true,
            MessageSize = largeMessage.Length,
            ContentType = "application/json",
            MessageId = "msg-large-001",
            ProcessedSuccessfully = true,
            AppliedReplacements = new List<string>
            {
                "PatientId: P12345 -> ${PatientId}",
                $"LargeDataSet: {sizeMultiplier * 100} fields -> ${{LargeDataSet}}"
            }
        };
    }

    /// <summary>
    /// Creates a collection of diverse events for comprehensive testing
    /// </summary>
    public static List<ConsumedEvent> CreateDiverseEventSet(DateTime? baseTimestamp = null)
    {
        var baseTime = baseTimestamp ?? DateTime.Now;
        
        return new List<ConsumedEvent>
        {
            CreatePatientRegistrationEvent(baseTime),
            CreateAppointmentEvent(baseTime.AddMinutes(2)),
            CreateBillingEvent(baseTime.AddMinutes(5)),
            CreateSystemAlertEvent(baseTime.AddMinutes(8)),
            CreateSpecialCharactersEvent(baseTime.AddMinutes(10)),
            CreateFailedEvent(baseTime.AddMinutes(12)),
            CreateMinimalEvent(baseTime.AddMinutes(15))
        };
    }

    /// <summary>
    /// Creates a sequence of related events for workflow testing
    /// </summary>
    public static List<ConsumedEvent> CreatePatientWorkflowEvents(DateTime? baseTimestamp = null)
    {
        var baseTime = baseTimestamp ?? DateTime.Now;
        var patientId = "P" + Random.Shared.Next(10000, 99999);
        
        return new List<ConsumedEvent>
        {
            // Patient registration
            new ConsumedEvent
            {
                Timestamp = baseTime,
                Exchange = "hospital.integration",
                RoutingKey = "patient.registered",
                Message = $$$"""{"patientId": "{{{patientId}}}", "firstName": "John", "lastName": "Doe", "action": "register"}""",
                ProcessedMessage = """{"patientId": "${PatientId}", "firstName": "${FirstName}", "lastName": "${LastName}", "action": "${Action}"}""",
                HasReplacements = true,
                IsJson = true,
                MessageSize = 89,
                MessageId = $"msg-{patientId}-reg"
            },
            
            // Appointment scheduled
            new ConsumedEvent
            {
                Timestamp = baseTime.AddMinutes(5),
                Exchange = "hospital.scheduling",
                RoutingKey = "appointment.scheduled",
                Message = $$$"""{"patientId": "{{{patientId}}}", "appointmentId": "APT001", "dateTime": "2024-01-16T10:00:00", "type": "consultation"}""",
                ProcessedMessage = """{"patientId": "${PatientId}", "appointmentId": "${AppointmentId}", "dateTime": "${AppointmentDateTime}", "type": "${AppointmentType}"}""",
                HasReplacements = true,
                IsJson = true,
                MessageSize = 112,
                MessageId = $"msg-{patientId}-apt"
            },
            
            // Treatment recorded
            new ConsumedEvent
            {
                Timestamp = baseTime.AddMinutes(30),
                Exchange = "medical.records",
                RoutingKey = "treatment.completed",
                Message = $$$"""{"patientId": "{{{patientId}}}", "treatmentId": "TRT001", "diagnosis": "Routine checkup", "status": "completed"}""",
                HasReplacements = false,
                IsJson = true,
                MessageSize = 108,
                MessageId = $"msg-{patientId}-trt"
            },
            
            // Invoice generated
            new ConsumedEvent
            {
                Timestamp = baseTime.AddMinutes(35),
                Exchange = "billing.events",
                RoutingKey = "invoice.created",
                Message = $$$"""{"patientId": "{{{patientId}}}", "invoiceId": "INV001", "amount": 150.00, "status": "pending"}""",
                ProcessedMessage = """{"patientId": "${PatientId}", "invoiceId": "${InvoiceId}", "amount": "${Amount}", "status": "${InvoiceStatus}"}""",
                HasReplacements = true,
                IsJson = true,
                MessageSize = 89,
                MessageId = $"msg-{patientId}-inv"
            }
        };
    }
}
