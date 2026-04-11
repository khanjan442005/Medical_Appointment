using Doctor_Appointment_System.Models;

namespace Doctor_Appointment_System.Services;

public interface IDemoPortalService
{
    DemoPatient GetCurrentPatient();
    DemoDoctor GetCurrentDoctor();
    DemoAdmin GetCurrentAdmin();
    IReadOnlyList<DemoPatient> GetPatients();
    IReadOnlyList<DemoDoctor> GetVerifiedDoctors();
    DemoDoctor? GetDoctor(int doctorId);
    DemoAppointment? GetAppointment(int appointmentId);
    DemoPatient? GetPatient(int patientId);
    IReadOnlyList<DemoAppointment> GetAppointmentsForPatient(int patientId);
    IReadOnlyList<DemoAppointment> GetAppointmentsForDoctor(int doctorId);
    IReadOnlyList<DemoAppointment> GetAllAppointments();
    IReadOnlyList<DemoNotification> GetNotificationsForPatient(int patientId);
    IReadOnlyList<DemoDoctorRequest> GetPendingDoctorRequests();
    IReadOnlyList<DemoDoctorRequest> GetAllDoctorRequests();
    IReadOnlyList<DoctorAvailabilitySlot> GetAvailabilitySlots(int doctorId);
    DemoAppointment CreatePendingAppointment(int patientId, int doctorId, DateOnly appointmentDate, string timeSlot);
    bool CompleteAppointmentPayment(int appointmentId, string paymentMethod);
    DemoDoctorRequest CreateDoctorRequest(string fullName, string email, string specialization, string licenseNumber, string password);
    DemoPatient CreatePatient(string fullName, string email, string password);
    DemoAdmin CreateAdmin(string fullName, string email, string password, string accessCode);
    DemoDoctor CreateVerifiedDoctor(string fullName, string email, string specialization, string licenseNumber, string password, int experienceYears, string hospitalName, string city, decimal consultationFee);
    (bool Success, string ErrorMessage) DeleteDoctor(int doctorId);
    bool ApproveDoctorRequest(int requestId);
    bool RejectDoctorRequest(int requestId);
    PortalMetrics GetPortalMetrics();
    Task<(bool Success, string ErrorMessage)> TryLoginAsync(string role, string email, string password);
    Task LogoutAsync();
    bool TryBeginPasswordReset(string email, out string normalizedEmail, out string errorMessage);
    bool ResetPassword(string email, string newPassword, out string errorMessage);
}
