using Doctor_Appointment_System.Models;
using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly IDemoPortalService _portalService;

        public PatientController(IDemoPortalService portalService)
        {
            _portalService = portalService;
        }

        public IActionResult Dashboard()
        {
            var patient = _portalService.GetCurrentPatient();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var appointments = _portalService.GetAppointmentsForPatient(patient.Id);
            var upcomingAppointments = appointments
                .Where(appointment => appointment.AppointmentDate >= today)
                .OrderBy(appointment => appointment.AppointmentDate)
                .ThenBy(appointment => appointment.TimeSlot)
                .ToList();

            var model = new PatientDashboardViewModel
            {
                Patient = patient,
                UpcomingAppointmentsCount = upcomingAppointments.Count,
                TotalVisitsCount = appointments.Count,
                SavedDoctorsCount = _portalService.GetVerifiedDoctors().Count,
                MedicalRecordsCount = Math.Max(appointments.Count(appointment => appointment.Status == "Completed") + 4, 4),
                UpcomingAppointments = upcomingAppointments.Take(3).Select(MapAppointmentCard).ToList(),
                FeaturedDoctors = _portalService.GetVerifiedDoctors().Take(2).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult FindDoctor(string? searchTerm, string? specialization)
        {
            var allDoctors = _portalService.GetVerifiedDoctors();
            var doctors = allDoctors
                .Where(doctor => string.IsNullOrWhiteSpace(searchTerm)
                    || doctor.FullName.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase)
                    || doctor.Specialization.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase)
                    || doctor.HospitalName.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase)
                    || doctor.City.Contains(searchTerm.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(doctor => string.IsNullOrWhiteSpace(specialization)
                    || string.Equals(doctor.Specialization, specialization.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var model = new FindDoctorViewModel
            {
                Doctors = doctors,
                VerifiedDoctorsCount = allDoctors.Count,
                PendingReviewCount = _portalService.GetPendingDoctorRequests().Count,
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                SelectedSpecialization = specialization?.Trim() ?? string.Empty,
                Specializations = allDoctors
                    .Select(doctor => doctor.Specialization)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult BookAppointment(int? doctorId)
        {
            var doctor = ResolveDoctor(doctorId);
            if (doctor is null)
            {
                return RedirectToAction(nameof(FindDoctor));
            }

            return View(BuildBookingViewModel(doctor, null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookAppointment(BookAppointmentInputModel input)
        {
            var doctor = _portalService.GetDoctor(input.DoctorId);
            if (doctor is null)
            {
                return RedirectToAction(nameof(FindDoctor));
            }

            if (!ModelState.IsValid)
            {
                return View(BuildBookingViewModel(doctor, input));
            }

            var appointmentDate = DateOnly.Parse(input.AppointmentDate);

            if (string.IsNullOrWhiteSpace(input.TimeSlot) || !doctor.AvailableSlots.Contains(input.TimeSlot))
            {
                ModelState.AddModelError(nameof(BookAppointmentInputModel.TimeSlot), "Please select a valid time slot.");
                return View(BuildBookingViewModel(doctor, input));
            }

            var patient = _portalService.GetCurrentPatient();
            try
            {
                var appointment = _portalService.CreatePendingAppointment(patient.Id, doctor.Id, appointmentDate, input.TimeSlot);
                return RedirectToAction(nameof(Payment), new { appointmentId = appointment.Id });
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(BuildBookingViewModel(doctor, input));
            }
        }

        public IActionResult BookingConfirm(int appointmentId)
        {
            var patient = _portalService.GetCurrentPatient();
            var appointment = _portalService.GetAppointment(appointmentId);
            if (appointment is null)
            {
                return RedirectToAction(nameof(AppointmentHistory));
            }

            if (appointment.PatientId != patient.Id)
            {
                return Forbid();
            }

            var doctor = _portalService.GetDoctor(appointment.DoctorId);
            if (doctor is null)
            {
                return RedirectToAction(nameof(AppointmentHistory));
            }

            return View(new BookingConfirmationViewModel
            {
                Appointment = appointment,
                Doctor = doctor
            });
        }

        public IActionResult DoctorProfile(int? id)
        {
            var doctor = ResolveDoctor(id);
            if (doctor is null)
            {
                return RedirectToAction(nameof(FindDoctor));
            }

            return View(new DoctorProfileViewModel
            {
                Doctor = doctor,
                AvailableSlots = doctor.AvailableSlots.ToList()
            });
        }

        public IActionResult AppointmentHistory()
        {
            var patient = _portalService.GetCurrentPatient();
            var model = new AppointmentHistoryViewModel
            {
                Appointments = _portalService.GetAppointmentsForPatient(patient.Id)
                    .Select(MapAppointmentCard)
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Notifications()
        {
            var patient = _portalService.GetCurrentPatient();
            var notifications = _portalService.GetNotificationsForPatient(patient.Id)
                .Select((notification, index) => new NotificationItemViewModel
                {
                    IndexLabel = (index + 1).ToString("D2"),
                    Title = notification.Title,
                    Message = notification.Message,
                    Label = notification.Label,
                    TimeLabel = notification.CreatedAt.ToString("dd MMM, hh:mm tt")
                })
                .ToList();

            return View(new NotificationsViewModel { Notifications = notifications });
        }

        [HttpGet]
        public IActionResult Payment(int appointmentId)
        {
            var currentPatient = _portalService.GetCurrentPatient();
            var appointment = _portalService.GetAppointment(appointmentId);
            var patient = appointment is null ? null : _portalService.GetPatient(appointment.PatientId);
            var doctor = appointment is null ? null : _portalService.GetDoctor(appointment.DoctorId);

            if (appointment is null || patient is null || doctor is null)
            {
                return RedirectToAction(nameof(AppointmentHistory));
            }

            if (appointment.PatientId != currentPatient.Id)
            {
                return Forbid();
            }

            return View(new PaymentViewModel
            {
                Appointment = appointment,
                Doctor = doctor,
                Patient = patient,
                Input = new PaymentInputModel
                {
                    AppointmentId = appointment.Id,
                    PaymentMethod = string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "UPI" : appointment.PaymentMethod
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Payment(PaymentInputModel input)
        {
            var currentPatient = _portalService.GetCurrentPatient();
            var appointment = _portalService.GetAppointment(input.AppointmentId);
            if (appointment is null)
            {
                return RedirectToAction(nameof(AppointmentHistory));
            }

            if (appointment.PatientId != currentPatient.Id)
            {
                return Forbid();
            }

            var patient = _portalService.GetPatient(appointment.PatientId)!;
            var doctor = _portalService.GetDoctor(appointment.DoctorId)!;
            if (!ModelState.IsValid)
            {
                return View(new PaymentViewModel
                {
                    Appointment = appointment,
                    Doctor = doctor,
                    Patient = patient,
                    Input = input
                });
            }

            if (!_portalService.CompleteAppointmentPayment(input.AppointmentId, input.PaymentMethod))
            {
                return RedirectToAction(nameof(AppointmentHistory));
            }

            return RedirectToAction(nameof(BookingConfirm), new { appointmentId = input.AppointmentId });
        }

        public IActionResult Profile()
        {
            var patient = _portalService.GetCurrentPatient();
            var appointments = _portalService.GetAppointmentsForPatient(patient.Id);
            var notifications = _portalService.GetNotificationsForPatient(patient.Id);

            return View(new PatientProfileViewModel
            {
                Patient = patient,
                ActiveAppointmentsCount = appointments.Count(appointment => appointment.AppointmentDate >= DateOnly.FromDateTime(DateTime.Today)),
                NotificationCount = notifications.Count
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View(new PatientRegistrationInputModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(PatientRegistrationInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                _portalService.CreatePatient(input.Name, input.Email, input.Password);
                TempData["Success"] = "Patient account created successfully. You can now sign in, and the admin user list has been updated.";
                return RedirectToAction("Login", "Auth");
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(input);
            }
        }

        private DemoDoctor? ResolveDoctor(int? doctorId)
        {
            if (doctorId.HasValue)
            {
                return _portalService.GetDoctor(doctorId.Value);
            }

            return _portalService.GetVerifiedDoctors().FirstOrDefault();
        }

        private BookAppointmentViewModel BuildBookingViewModel(DemoDoctor doctor, BookAppointmentInputModel? input)
        {
            return new BookAppointmentViewModel
            {
                Doctor = doctor,
                Input = input ?? new BookAppointmentInputModel { DoctorId = doctor.Id },
                AvailableSlots = doctor.AvailableSlots.ToList(),
                MinimumDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd")
            };
        }

        private AppointmentCardViewModel MapAppointmentCard(DemoAppointment appointment)
        {
            var doctor = _portalService.GetDoctor(appointment.DoctorId)!;
            var patient = _portalService.GetPatient(appointment.PatientId)!;

            return new AppointmentCardViewModel
            {
                Id = appointment.Id,
                DoctorName = doctor.FullName,
                DoctorSpecialization = doctor.Specialization,
                PatientName = patient.FullName,
                DateLabel = PortalFormatting.FormatDate(appointment.AppointmentDate),
                TimeSlot = appointment.TimeSlot,
                Status = appointment.Status,
                FeeLabel = PortalFormatting.FormatCurrency(appointment.TotalAmount),
                PaymentMethod = string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "Pending" : appointment.PaymentMethod
            };
        }
    }
}
