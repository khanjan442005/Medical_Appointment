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
            var metrics = _portalService.GetPortalMetrics();

            return View(new AdminDashboardViewModel
            {
                Metrics = metrics,
                RecentDoctorRequests = _portalService.GetAllDoctorRequests()
                    .Take(4)
                    .ToList(),
                RecentAppointments = BuildAdminAppointmentRows(_portalService.GetAllAppointments())
                    .Take(5)
                    .ToList()
            });
        }

        public IActionResult DoctorVerification(string? searchTerm, string? statusFilter)
        {
            var allRequests = _portalService.GetAllDoctorRequests();
            var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "Pending" : statusFilter.Trim();

            var filteredRequests = allRequests
                .Where(request => MatchesSearch(
                    normalizedSearch,
                    request.FullName,
                    request.Email,
                    request.Specialization,
                    request.LicenseNumber,
                    request.HospitalName,
                    request.City))
                .Where(request => MatchesFilter(request.Status, normalizedStatus))
                .ToList();

            return View(new DoctorVerificationViewModel
            {
                Requests = filteredRequests,
                VerifiedDoctorsCount = _portalService.GetVerifiedDoctors().Count,
                PendingRequestsCount = allRequests.Count(request => request.Status == "Pending"),
                ApprovedRequestsCount = allRequests.Count(request => request.Status == "Approved"),
                RejectedRequestsCount = allRequests.Count(request => request.Status == "Rejected"),
                SearchTerm = normalizedSearch,
                StatusFilter = normalizedStatus
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveDoctor(int requestId)
        {
            var success = _portalService.ApproveDoctorRequest(requestId);
            TempData[success ? "Success" : "Error"] = success
                ? "Doctor approved successfully. The patient directory and admin metrics have been updated."
                : "The request could not be processed.";

            return RedirectToAction(nameof(DoctorVerification));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectDoctor(int requestId)
        {
            var success = _portalService.RejectDoctorRequest(requestId);
            TempData[success ? "Success" : "Error"] = success
                ? "Doctor request rejected successfully."
                : "The request could not be processed.";

            return RedirectToAction(nameof(DoctorVerification));
        }

        public IActionResult Reports()
        {
            var metrics = _portalService.GetPortalMetrics();
            var appointments = _portalService.GetAllAppointments();
            var doctorRequests = _portalService.GetAllDoctorRequests();
            var doctors = _portalService.GetVerifiedDoctors();

            return View(new ReportsViewModel
            {
                Metrics = metrics,
                AppointmentStatusBreakdown = appointments
                    .GroupBy(appointment => appointment.Status)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{PortalFormatting.FormatCurrency(group.Sum(item => item.TotalAmount))} linked to this status"
                    })
                    .ToList(),
                RequestStatusBreakdown = doctorRequests
                    .GroupBy(request => request.Status)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{group.Select(item => item.City).Distinct(StringComparer.OrdinalIgnoreCase).Count()} cities represented"
                    })
                    .ToList(),
                PaymentMethodBreakdown = appointments
                    .Where(appointment => appointment.PaymentCompleted)
                    .GroupBy(appointment => string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "Pending" : appointment.PaymentMethod)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{PortalFormatting.FormatCurrency(group.Sum(item => item.TotalAmount))} collected"
                    })
                    .ToList(),
                SpecializationBreakdown = doctors
                    .GroupBy(doctor => doctor.Specialization)
                    .OrderByDescending(group => group.Count())
                    .Take(5)
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{PortalFormatting.FormatCurrency(group.Average(item => item.ConsultationFee))} avg fee"
                    })
                    .ToList()
            });
        }

        public IActionResult Appointments(string? searchTerm, string? statusFilter, string? paymentFilter)
        {
            var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter.Trim();
            var normalizedPayment = string.IsNullOrWhiteSpace(paymentFilter) ? "All" : paymentFilter.Trim();

            var appointments = BuildAdminAppointmentRows(_portalService.GetAllAppointments())
                .Where(appointment => MatchesSearch(
                    normalizedSearch,
                    appointment.PatientName,
                    appointment.DoctorName,
                    appointment.DoctorSpecialization,
                    appointment.DateLabel,
                    appointment.TimeSlot,
                    appointment.Status,
                    appointment.PaymentMethod))
                .Where(appointment => MatchesFilter(appointment.Status, normalizedStatus))
                .Where(appointment => MatchesFilter(appointment.PaymentStatus, normalizedPayment))
                .ToList();

            return View(new AdminAppointmentsViewModel
            {
                Appointments = appointments,
                SearchTerm = normalizedSearch,
                StatusFilter = normalizedStatus,
                PaymentFilter = normalizedPayment,
                TotalAppointmentsCount = appointments.Count,
                ConfirmedAppointmentsCount = appointments.Count(appointment => appointment.Status == "Confirmed"),
                CompletedAppointmentsCount = appointments.Count(appointment => appointment.Status == "Completed"),
                PendingPaymentsCount = appointments.Count(appointment => appointment.PaymentStatus == "Pending"),
                RevenueLabel = PortalFormatting.FormatCurrency(appointments.Sum(appointment => appointment.TotalAmount))
            });
        }

        public IActionResult UserManagement(string? searchTerm, string? roleFilter, string? statusFilter)
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

            var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
            var normalizedRole = string.IsNullOrWhiteSpace(roleFilter) ? "All" : roleFilter.Trim();
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter.Trim();

            var filteredUsers = users
                .Where(user => MatchesSearch(normalizedSearch, user.Name, user.Detail, user.Role, user.Status))
                .Where(user => MatchesFilter(user.Role, normalizedRole))
                .Where(user => MatchesFilter(user.Status, normalizedStatus))
                .OrderBy(user => user.Role == "Doctor" ? 0 : 1)
                .ThenBy(user => user.Status)
                .ThenBy(user => user.Name)
                .ToList();

            return View(new UserManagementViewModel
            {
                Users = filteredUsers,
                SearchTerm = normalizedSearch,
                RoleFilter = normalizedRole,
                StatusFilter = normalizedStatus,
                PatientCount = users.Count(user => user.Role == "Patient"),
                VerifiedDoctorCount = users.Count(user => user.Role == "Doctor" && user.Status == "Verified"),
                PendingDoctorCount = users.Count(user => user.Role == "Doctor" && user.Status == "Pending")
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

        private List<AdminAppointmentRowViewModel> BuildAdminAppointmentRows(IEnumerable<DemoAppointment> appointments)
        {
            var patientsById = _portalService.GetPatients().ToDictionary(patient => patient.Id);
            var doctorsById = _portalService.GetVerifiedDoctors().ToDictionary(doctor => doctor.Id);

            return appointments
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
                        PaymentStatus = appointment.PaymentCompleted ? "Paid" : "Pending",
                        TotalAmount = appointment.TotalAmount,
                        FeeLabel = PortalFormatting.FormatCurrency(appointment.TotalAmount),
                        CreatedAtLabel = appointment.CreatedAt.ToLocalTime().ToString("dd MMM yyyy")
                    };
                })
                .ToList();
        }

        private static bool MatchesSearch(string searchTerm, params string?[] fields)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return true;
            }

            return fields.Any(field => !string.IsNullOrWhiteSpace(field) && field.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        private static bool MatchesFilter(string value, string filter) =>
            string.IsNullOrWhiteSpace(filter)
            || string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);
    }
}
