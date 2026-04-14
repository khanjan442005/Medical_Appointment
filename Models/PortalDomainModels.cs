using System.Globalization;

namespace Doctor_Appointment_System.Models;

public static class PortalFormatting
{
    public static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "MC";
        }

        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0]));

        return string.Concat(parts);
    }

    public static string FormatCurrency(decimal amount) => $"INR {amount:N0}";

    public static string FormatDate(DateOnly date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
}

public sealed class DemoPatient
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DateOfBirthLabel { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = "Active patient";
    public string Initials => PortalFormatting.GetInitials(FullName);
}

public sealed class DemoDoctor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public decimal Rating { get; set; }
    public bool IsVerified { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public List<string> AvailableSlots { get; set; } = [];
    public string Initials => PortalFormatting.GetInitials(FullName.Replace("Dr. ", string.Empty, StringComparison.OrdinalIgnoreCase));
}

public sealed class DemoAdmin
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Initials => PortalFormatting.GetInitials(FullName);
}

public sealed class DemoAppointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public decimal PlatformFee { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public bool PaymentCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount => ConsultationFee + PlatformFee;
}

public sealed class DemoNotification
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class DemoDoctorRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}

public sealed class DoctorAvailabilitySlot
{
    public string DayLabel { get; set; } = string.Empty;
    public string SessionLabel { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public List<string> SlotValues { get; set; } = [];
}

public sealed class PortalMetrics
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingDoctorVerifications { get; set; }
    public int PendingPayments { get; set; }
    public int ActiveDoctors { get; set; }
    public int WeeklyAppointments { get; set; }
    public int NewRegistrations { get; set; }
    public string ApprovalRateLabel { get; set; } = string.Empty;
    public string RevenueLabel { get; set; } = string.Empty;
    public string RevenueTrendLabel { get; set; } = string.Empty;
}
