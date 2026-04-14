using System.ComponentModel.DataAnnotations;

namespace Doctor_Appointment_System.Models;

public sealed class AppointmentCardViewModel
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string FeeLabel { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string CreatedAtLabel { get; set; } = string.Empty;
}

public sealed class NotificationItemViewModel
{
    public string IndexLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
}

public sealed class UserRowViewModel
{
    public int EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public bool CanDelete { get; set; }
}

public sealed class BookAppointmentInputModel : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a doctor.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Please select an appointment date.")]
    public string AppointmentDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a time slot.")]
    public string TimeSlot { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!DateOnly.TryParse(AppointmentDate, out var appointmentDate))
        {
            yield return new ValidationResult("Please select a valid appointment date.", [nameof(AppointmentDate)]);
            yield break;
        }

        if (appointmentDate < DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult("Appointments cannot be booked for a past date.", [nameof(AppointmentDate)]);
        }
    }
}

public sealed class PaymentInputModel : IValidatableObject
{
    public static IReadOnlyList<string> SupportedMethods { get; } = ["Card", "UPI", "Net Banking", "Cash at clinic"];

    [Range(1, int.MaxValue, ErrorMessage = "Appointment not found.")]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Please select a payment method.")]
    public string PaymentMethod { get; set; } = "UPI";
    public string CardNumber { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public string Cvc { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!SupportedMethods.Contains(PaymentMethod, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Please select a valid payment method.", [nameof(PaymentMethod)]);
            yield break;
        }

        if (!string.Equals(PaymentMethod, "Card", StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(CardNumber))
        {
            yield return new ValidationResult("Card number is required for card payments.", [nameof(CardNumber)]);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(CardNumber.Trim(), @"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$"))
        {
            yield return new ValidationResult("Enter a valid 16-digit card number.", [nameof(CardNumber)]);
        }

        if (string.IsNullOrWhiteSpace(Expiry))
        {
            yield return new ValidationResult("Expiry is required for card payments.", [nameof(Expiry)]);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Expiry.Trim(), @"^(0[1-9]|1[0-2])/\d{2}$"))
        {
            yield return new ValidationResult("Enter expiry in MM/YY format.", [nameof(Expiry)]);
        }

        if (string.IsNullOrWhiteSpace(Cvc))
        {
            yield return new ValidationResult("CVC is required for card payments.", [nameof(Cvc)]);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Cvc.Trim(), @"^\d{3,4}$"))
        {
            yield return new ValidationResult("Enter a valid 3 or 4 digit CVC.", [nameof(Cvc)]);
        }
    }
}

public sealed class PatientDashboardViewModel
{
    public DemoPatient Patient { get; set; } = new();
    public int UpcomingAppointmentsCount { get; set; }
    public int TotalVisitsCount { get; set; }
    public int SavedDoctorsCount { get; set; }
    public int MedicalRecordsCount { get; set; }
    public List<AppointmentCardViewModel> UpcomingAppointments { get; set; } = [];
    public List<DemoDoctor> FeaturedDoctors { get; set; } = [];
}

public sealed class FindDoctorViewModel
{
    public List<DemoDoctor> Doctors { get; set; } = [];
    public int VerifiedDoctorsCount { get; set; }
    public int PendingReviewCount { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedSpecialization { get; set; } = string.Empty;
    public List<string> Specializations { get; set; } = [];
}

public sealed class DoctorProfileViewModel
{
    public DemoDoctor Doctor { get; set; } = new();
    public List<string> AvailableSlots { get; set; } = [];
}

public sealed class BookAppointmentViewModel
{
    public DemoDoctor Doctor { get; set; } = new();
    public BookAppointmentInputModel Input { get; set; } = new();
    public List<string> AvailableSlots { get; set; } = [];
    public string MinimumDate { get; set; } = string.Empty;
}

public sealed class PaymentViewModel
{
    public DemoAppointment Appointment { get; set; } = new();
    public DemoDoctor Doctor { get; set; } = new();
    public DemoPatient Patient { get; set; } = new();
    public PaymentInputModel Input { get; set; } = new();
}

public sealed class BookingConfirmationViewModel
{
    public DemoAppointment Appointment { get; set; } = new();
    public DemoDoctor Doctor { get; set; } = new();
}

public sealed class AppointmentHistoryViewModel
{
    public List<AppointmentCardViewModel> Appointments { get; set; } = [];
}

public sealed class NotificationsViewModel
{
    public List<NotificationItemViewModel> Notifications { get; set; } = [];
}

public sealed class PatientProfileViewModel
{
    public DemoPatient Patient { get; set; } = new();
    public int ActiveAppointmentsCount { get; set; }
    public int NotificationCount { get; set; }
}

public sealed class DoctorDashboardViewModel
{
    public DemoDoctor Doctor { get; set; } = new();
    public int TodaysAppointmentsCount { get; set; }
    public int TotalPatientsCount { get; set; }
    public int PendingActionsCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public string MonthlyEarningsLabel { get; set; } = string.Empty;
    public string NextAvailabilityLabel { get; set; } = string.Empty;
    public List<AppointmentCardViewModel> UpcomingAppointments { get; set; } = [];
    public List<DoctorAvailabilitySlot> AvailabilityPreview { get; set; } = [];
}

public sealed class DoctorAppointmentsViewModel
{
    public List<AppointmentCardViewModel> Appointments { get; set; } = [];
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "All";
    public string PaymentFilter { get; set; } = "All";
    public int TotalAppointmentsCount { get; set; }
    public int ConfirmedAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public string RevenueLabel { get; set; } = string.Empty;
}

public sealed class DoctorAvailabilityViewModel
{
    public List<DoctorAvailabilitySlot> Slots { get; set; } = [];
    public int TotalSlotGroupsCount { get; set; }
    public int ActiveDaysCount { get; set; }
    public string NextAvailabilityLabel { get; set; } = string.Empty;
    public List<ReportBreakdownItemViewModel> DayBreakdown { get; set; } = [];
}

public sealed class DoctorEarningsViewModel
{
    public string TotalEarningsLabel { get; set; } = string.Empty;
    public string ThisMonthLabel { get; set; } = string.Empty;
    public string AveragePerVisitLabel { get; set; } = string.Empty;
    public string PendingPayoutLabel { get; set; } = string.Empty;
    public int PaidAppointmentsCount { get; set; }
    public int PendingPaymentCount { get; set; }
    public string RevenueTrendLabel { get; set; } = string.Empty;
    public List<ReportBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = [];
    public List<ReportBreakdownItemViewModel> StatusBreakdown { get; set; } = [];
}

public sealed class DoctorOwnProfileViewModel
{
    public DemoDoctor Doctor { get; set; } = new();
    public int TotalAppointmentsCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int DistinctPatientsCount { get; set; }
    public int TotalAvailabilityBlocks { get; set; }
    public string TotalEarningsLabel { get; set; } = string.Empty;
}

public sealed class AdminDashboardViewModel
{
    public PortalMetrics Metrics { get; set; } = new();
    public List<DemoDoctorRequest> RecentDoctorRequests { get; set; } = [];
    public List<AdminAppointmentRowViewModel> RecentAppointments { get; set; } = [];
}

public sealed class DoctorVerificationViewModel
{
    public List<DemoDoctorRequest> Requests { get; set; } = [];
    public int VerifiedDoctorsCount { get; set; }
    public int PendingRequestsCount { get; set; }
    public int ApprovedRequestsCount { get; set; }
    public int RejectedRequestsCount { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "Pending";
}

public sealed class UserManagementViewModel
{
    public List<UserRowViewModel> Users { get; set; } = [];
    public string SearchTerm { get; set; } = string.Empty;
    public string RoleFilter { get; set; } = "All";
    public string StatusFilter { get; set; } = "All";
    public int PatientCount { get; set; }
    public int VerifiedDoctorCount { get; set; }
    public int PendingDoctorCount { get; set; }
}

public sealed class ReportsViewModel
{
    public PortalMetrics Metrics { get; set; } = new();
    public List<ReportBreakdownItemViewModel> AppointmentStatusBreakdown { get; set; } = [];
    public List<ReportBreakdownItemViewModel> RequestStatusBreakdown { get; set; } = [];
    public List<ReportBreakdownItemViewModel> PaymentMethodBreakdown { get; set; } = [];
    public List<ReportBreakdownItemViewModel> SpecializationBreakdown { get; set; } = [];
}

public sealed class AdminAppointmentRowViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string FeeLabel { get; set; } = string.Empty;
    public string CreatedAtLabel { get; set; } = string.Empty;
}

public sealed class AdminAppointmentsViewModel
{
    public List<AdminAppointmentRowViewModel> Appointments { get; set; } = [];
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "All";
    public string PaymentFilter { get; set; } = "All";
    public int TotalAppointmentsCount { get; set; }
    public int ConfirmedAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int PendingPaymentsCount { get; set; }
    public string RevenueLabel { get; set; } = string.Empty;
}

public sealed class ReportBreakdownItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ValueLabel { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}
