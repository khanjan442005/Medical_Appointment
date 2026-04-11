namespace Doctor_Appointment_System.Data;

public sealed class PatientEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string AvatarPath { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = "Active patient";
    public DateTime CreatedAt { get; set; }
    public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
    public ICollection<NotificationEntity> Notifications { get; set; } = new List<NotificationEntity>();
}

public sealed class DoctorEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
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
    public DateTime CreatedAt { get; set; }
    public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
    public ICollection<DoctorAvailabilityEntity> AvailabilitySlots { get; set; } = new List<DoctorAvailabilityEntity>();
}

public sealed class AdminEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class DoctorRequestEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}

public sealed class DoctorAvailabilityEntity
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string SessionLabel { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string SlotValue { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DoctorEntity Doctor { get; set; } = null!;
}

public sealed class AppointmentEntity
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
    public PatientEntity Patient { get; set; } = null!;
    public DoctorEntity Doctor { get; set; } = null!;
}

public sealed class NotificationEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public PatientEntity Patient { get; set; } = null!;
}
