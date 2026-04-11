using Doctor_Appointment_System.Models;
using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDemoPortalService _portalService;

        public AdminController(IDemoPortalService portalService)
        {
            _portalService = portalService;
        }

        public IActionResult Dashboard()
        {
            return View(new AdminDashboardViewModel
            {
                Metrics = _portalService.GetPortalMetrics()
            });
        }

        public IActionResult DoctorVerification()
        {
            return View(new DoctorVerificationViewModel
            {
                PendingRequests = _portalService.GetPendingDoctorRequests().ToList(),
                VerifiedDoctorsCount = _portalService.GetVerifiedDoctors().Count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveDoctor(int requestId)
        {
            TempData["Success"] = _portalService.ApproveDoctorRequest(requestId)
                ? "Doctor approved successfully. The patient directory and admin metrics have been updated."
                : "The request could not be processed.";

            return RedirectToAction(nameof(DoctorVerification));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectDoctor(int requestId)
        {
            TempData["Success"] = _portalService.RejectDoctorRequest(requestId)
                ? "Doctor request rejected successfully."
                : "The request could not be processed.";

            return RedirectToAction(nameof(DoctorVerification));
        }

        public IActionResult Reports()
        {
            return View(new ReportsViewModel
            {
                Metrics = _portalService.GetPortalMetrics()
            });
        }

        public IActionResult Appointments()
        {
            var patientsById = _portalService.GetPatients().ToDictionary(patient => patient.Id);
            var doctorsById = _portalService.GetVerifiedDoctors().ToDictionary(doctor => doctor.Id);
            var appointments = _portalService.GetAllAppointments()
                .Select(appointment =>
                {
                    patientsById.TryGetValue(appointment.PatientId, out var patient);
                    doctorsById.TryGetValue(appointment.DoctorId, out var doctor);

                    return new AdminAppointmentRowViewModel
                    {
                        Id = appointment.Id,
                        PatientName = patient?.FullName ?? "Unknown patient",
                        DoctorName = doctor?.FullName ?? "Unknown doctor",
                        DoctorSpecialization = doctor?.Specialization ?? "Unavailable",
                        DateLabel = PortalFormatting.FormatDate(appointment.AppointmentDate),
                        TimeSlot = appointment.TimeSlot,
                        Status = appointment.Status,
                        PaymentMethod = string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "Pending" : appointment.PaymentMethod,
                        FeeLabel = PortalFormatting.FormatCurrency(appointment.TotalAmount)
                    };
                })
                .ToList();

            return View(new AdminAppointmentsViewModel
            {
                Appointments = appointments
            });
        }

        public IActionResult UserManagement(string? searchTerm, string? roleFilter)
        {
            var users = new List<UserRowViewModel>();
            users.AddRange(_portalService.GetPatients().Select(patient => new UserRowViewModel
            {
                EntityId = patient.Id,
                Name = patient.FullName,
                Role = "Patient",
                Status = "Active",
                Detail = patient.Email
            }));

            users.AddRange(_portalService.GetVerifiedDoctors().Select(doctor => new UserRowViewModel
            {
                EntityId = doctor.Id,
                Name = doctor.FullName,
                Role = "Doctor",
                Status = "Verified",
                Detail = doctor.Specialization,
                CanDelete = true
            }));

            users.AddRange(_portalService.GetPendingDoctorRequests().Select(request => new UserRowViewModel
            {
                EntityId = request.Id,
                Name = request.FullName,
                Role = "Doctor",
                Status = "Pending",
                Detail = request.Specialization
            }));

            var filteredUsers = users
                .Where(user => string.IsNullOrWhiteSpace(searchTerm)
                    || user.Name.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase)
                    || user.Detail.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(user => string.IsNullOrWhiteSpace(roleFilter)
                    || string.Equals(roleFilter.Trim(), "All", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(user.Role, roleFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.Role)
                .ThenBy(user => user.Name)
                .ToList();

            return View(new UserManagementViewModel
            {
                Users = filteredUsers,
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                RoleFilter = string.IsNullOrWhiteSpace(roleFilter) ? "All" : roleFilter.Trim()
            });
        }

        [HttpGet]
        public IActionResult AddDoctor() => View(new AdminCreateDoctorInputModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctor(AdminCreateDoctorInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                _portalService.CreateVerifiedDoctor(
                    input.Name,
                    input.Email,
                    input.Specialization,
                    input.License,
                    input.Password,
                    input.ExperienceYears,
                    input.HospitalName,
                    input.City,
                    input.ConsultationFee);

                TempData["Success"] = "Doctor account created successfully and is now available in the live directory.";
                return RedirectToAction(nameof(UserManagement));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(input);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDoctor(int doctorId)
        {
            var (success, errorMessage) = _portalService.DeleteDoctor(doctorId);
            TempData[success ? "Success" : "Error"] = success
                ? "Doctor deleted successfully."
                : errorMessage;

            return RedirectToAction(nameof(UserManagement));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View(new AdminRegistrationInputModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AdminRegistrationInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                _portalService.CreateAdmin(input.Name, input.Email, input.Password, input.AdminCode);
                TempData["Success"] = "Admin account created successfully. You can now sign in to the dashboard.";
                return RedirectToAction("Login", "Auth");
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(input);
            }
        }
    }
}
