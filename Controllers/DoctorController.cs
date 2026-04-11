using Doctor_Appointment_System.Models;
using Doctor_Appointment_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doctor_Appointment_System.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDemoPortalService _portalService;

        public DoctorController(IDemoPortalService portalService)
        {
            _portalService = portalService;
        }

        public IActionResult Dashboard()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var appointments = _portalService.GetAppointmentsForDoctor(doctor.Id);
            var monthlyRevenue = appointments
                .Where(appointment => appointment.PaymentCompleted && appointment.AppointmentDate.Year == today.Year && appointment.AppointmentDate.Month == today.Month)
                .Sum(appointment => appointment.TotalAmount);

            var model = new DoctorDashboardViewModel
            {
                Doctor = doctor,
                TodaysAppointmentsCount = appointments.Count(appointment => appointment.AppointmentDate == today),
                TotalPatientsCount = appointments.Select(appointment => appointment.PatientId).Distinct().Count(),
                PendingActionsCount = appointments.Count(appointment => appointment.Status == "Payment Pending"),
                MonthlyEarningsLabel = PortalFormatting.FormatCurrency(monthlyRevenue),
                UpcomingAppointments = appointments
                    .Where(appointment => appointment.AppointmentDate >= today)
                    .Take(4)
                    .Select(MapAppointmentCard)
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Appointments()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var model = new DoctorAppointmentsViewModel
            {
                Appointments = _portalService.GetAppointmentsForDoctor(doctor.Id)
                    .Select(MapAppointmentCard)
                    .ToList()
            };

            return View(model);
        }

        public IActionResult Profile()
        {
            return View(new DoctorOwnProfileViewModel
            {
                Doctor = _portalService.GetCurrentDoctor()
            });
        }

        public IActionResult Availability()
        {
            var doctor = _portalService.GetCurrentDoctor();
            return View(new DoctorAvailabilityViewModel
            {
                Slots = _portalService.GetAvailabilitySlots(doctor.Id).ToList()
            });
        }

        public IActionResult Earnings()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var appointments = _portalService.GetAppointmentsForDoctor(doctor.Id);
            var paidAppointments = appointments.Where(appointment => appointment.PaymentCompleted).ToList();

            var model = new DoctorEarningsViewModel
            {
                TotalEarningsLabel = PortalFormatting.FormatCurrency(paidAppointments.Sum(appointment => appointment.TotalAmount)),
                ThisMonthLabel = PortalFormatting.FormatCurrency(paidAppointments
                    .Where(appointment => appointment.AppointmentDate.Year == today.Year && appointment.AppointmentDate.Month == today.Month)
                    .Sum(appointment => appointment.TotalAmount)),
                AveragePerVisitLabel = paidAppointments.Count == 0
                    ? "INR 0"
                    : PortalFormatting.FormatCurrency(paidAppointments.Average(appointment => appointment.TotalAmount)),
                PendingPayoutLabel = PortalFormatting.FormatCurrency(appointments
                    .Where(appointment => !appointment.PaymentCompleted)
                    .Sum(appointment => appointment.TotalAmount))
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View(new DoctorRegistrationInputModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(DoctorRegistrationInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                _portalService.CreateDoctorRequest(input.Name, input.Email, input.Specialization, input.License, input.Password);
                TempData["Success"] = "Doctor registration request submitted successfully. The admin verification queue has been updated.";
                return RedirectToAction("Login", "Auth");
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(input);
            }
        }

        private AppointmentCardViewModel MapAppointmentCard(DemoAppointment appointment)
        {
            var patient = _portalService.GetPatient(appointment.PatientId)!;
            var doctor = _portalService.GetCurrentDoctor();

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
