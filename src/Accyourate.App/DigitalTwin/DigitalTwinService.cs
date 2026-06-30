using Accyourate.App.Data;

namespace Accyourate.App.DigitalTwin;

public sealed class DigitalTwinService
{
    private readonly DatabaseService _database;

    public DigitalTwinService(DatabaseService database)
    {
        _database = database;
    }

    public IReadOnlyList<DigitalTwinDeviceRecord> GetDevices()
    {
        var devices = new List<DigitalTwinDeviceRecord>
        {
            new()
            {
                Code = "TOP001",
                Type = "Smart Textile",
                Model = "ECG Textile Top",
                SerialNumber = "SN-TOP-001",
                Rfid = "RFID-TP01",
                Status = "Online",
                BatteryLevel = 87,
                HeartRate = 72,
                EcgStatus = "Normale",
                SignalQuality = "Ottima",
                Firmware = "1.0.0",
                AssignedTo = "Demo Patient"
            },
            new()
            {
                Code = "CU001",
                Type = "Control Unit",
                Model = "CU-ECG-01",
                SerialNumber = "SN-CU-001",
                Rfid = "RFID-CU01",
                Status = "Online",
                BatteryLevel = 64,
                HeartRate = 78,
                EcgStatus = "Normale",
                SignalQuality = "Buona",
                Firmware = "1.0.0",
                AssignedTo = "Demo Patient"
            },
            new()
            {
                Code = "TOP002",
                Type = "Smart Textile",
                Model = "ECG Textile Top",
                SerialNumber = "SN-TOP-002",
                Rfid = "RFID-TP02",
                Status = "Offline",
                BatteryLevel = 18,
                HeartRate = 0,
                EcgStatus = "Non disponibile",
                SignalQuality = "Assente",
                Firmware = "1.0.0",
                AssignedTo = "Non assegnato"
            }
        };

        return devices;
    }

    public IReadOnlyList<DigitalTwinTelemetryRecord> GetTelemetry()
    {
        return new List<DigitalTwinTelemetryRecord>
        {
            new() { DeviceCode = "TOP001", HeartRate = 72, BatteryLevel = 87, EcgStatus = "Normale", SignalQuality = "Ottima", EventType = "ECG_RECEIVED", Timestamp = DateTime.Now.AddMinutes(-4) },
            new() { DeviceCode = "CU001", HeartRate = 78, BatteryLevel = 64, EcgStatus = "Normale", SignalQuality = "Buona", EventType = "HEART_RATE_RECEIVED", Timestamp = DateTime.Now.AddMinutes(-8) },
            new() { DeviceCode = "TOP002", HeartRate = 0, BatteryLevel = 18, EcgStatus = "Offline", SignalQuality = "Assente", EventType = "LOW_BATTERY", Timestamp = DateTime.Now.AddMinutes(-18) }
        };
    }

    public int CountOnline() => GetDevices().Count(d => d.Status == "Online");
    public int CountOffline() => GetDevices().Count(d => d.Status == "Offline");
    public int CountLowBattery() => GetDevices().Count(d => d.BatteryLevel < 20);
    public int CountEcgNormal() => GetDevices().Count(d => d.EcgStatus == "Normale");
}
