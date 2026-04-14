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
            var slots = _portalService.GetAvailabilitySlots(doctor.Id).ToList();
            var appointmentCards = BuildAppointmentCards(doctor, appointments);
            var monthlyRevenue = appointments
                .Where(appointment => appointment.PaymentCompleted && appointment.AppointmentDate.Year == today.Year && appointment.AppointmentDate.Month == today.Month)
                .Sum(appointment => appointment.TotalAmount);

            var model = new DoctorDashboardViewModel
            {
                Doctor = doctor,
                TodaysAppointmentsCount = appointments.Count(appointment => appointment.AppointmentDate == today),
                TotalPatientsCount = appointments.Select(appointment => appointment.PatientId).Distinct().Count(),
                PendingActionsCount = appointments.Count(appointment => appointment.Status == "Payment Pending"),
                UpcomingAppointmentsCount = appointments.Count(appointment => appointment.AppointmentDate >= today),
                CompletedAppointmentsCount = appointments.Count(appointment => appointment.Status == "Completed"),
                MonthlyEarningsLabel = PortalFormatting.FormatCurrency(monthlyRevenue),
                NextAvailabilityLabel = slots.Count == 0 ? "No schedule saved" : $"{slots[0].DayLabel} • {slots[0].TimeRange}",
                UpcomingAppointments = appointmentCards
                    .Where(appointment => appointments.First(item => item.Id == appointment.Id).AppointmentDate >= today)
                    .Take(4)
                    .ToList(),
                AvailabilityPreview = slots.Take(3).ToList()
            };

            return View(model);
        }

        public IActionResult Appointments(string? searchTerm, string? statusFilter, string? paymentFilter)
        {
            var doctor = _portalService.GetCurrentDoctor();
            var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
            var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter.Trim();
            var normalizedPayment = string.IsNullOrWhiteSpace(paymentFilter) ? "All" : paymentFilter.Trim();
            var appointmentCards = BuildAppointmentCards(doctor, _portalService.GetAppointmentsForDoctor(doctor.Id));

            var model = new DoctorAppointmentsViewModel
            {
                Appointments = appointmentCards
                    .Where(appointment => MatchesSearch(
                        normalizedSearch,
                        appointment.PatientName,
                        appointment.DateLabel,
                        appointment.TimeSlot,
                        appointment.Status,
                        appointment.PaymentMethod))
                    .Where(appointment => MatchesFilter(appointment.Status, normalizedStatus))
                    .Where(appointment => MatchesFilter(appointment.PaymentStatus, normalizedPayment))
                    .ToList(),
                SearchTerm = normalizedSearch,
                StatusFilter = normalizedStatus,
                PaymentFilter = normalizedPayment
            };

            model.TotalAppointmentsCount = model.Appointments.Count;
            model.ConfirmedAppointmentsCount = model.Appointments.Count(appointment => appointment.Status == "Confirmed");
            model.CompletedAppointmentsCount = model.Appointments.Count(appointment => appointment.Status == "Completed");
            model.PendingAppointmentsCount = model.Appointments.Count(appointment => appointment.Status == "Payment Pending");
            model.RevenueLabel = PortalFormatting.FormatCurrency(
                model.Appointments.Sum(appointment => ParseCurrency(appointment.FeeLabel)));

            return View(model);
        }

        public IActionResult Profile()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var appointments = _portalService.GetAppointmentsForDoctor(doctor.Id);
            var slots = _portalService.GetAvailabilitySlots(doctor.Id);

            return View(new DoctorOwnProfileViewModel
            {
                Doctor = doctor,
                TotalAppointmentsCount = appointments.Count,
                UpcomingAppointmentsCount = appointments.Count(appointment => appointment.AppointmentDate >= DateOnly.FromDateTime(DateTime.Today)),
                DistinctPatientsCount = appointments.Select(appointment => appointment.PatientId).Distinct().Count(),
                TotalAvailabilityBlocks = slots.Count,
                TotalEarningsLabel = PortalFormatting.FormatCurrency(appointments.Where(appointment => appointment.PaymentCompleted).Sum(appointment => appointment.TotalAmount))
            });
        }

        public IActionResult Availability()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var slots = _portalService.GetAvailabilitySlots(doctor.Id).ToList();
            return View(new DoctorAvailabilityViewModel
            {
                Slots = slots,
                TotalSlotGroupsCount = slots.Count,
                ActiveDaysCount = slots.Select(slot => slot.DayLabel).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                NextAvailabilityLabel = slots.Count == 0 ? "No schedule saved" : $"{slots[0].DayLabel} • {slots[0].TimeRange}",
                DayBreakdown = slots
                    .GroupBy(slot => slot.DayLabel)
                    .OrderBy(group => group.Key)
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = string.Join(", ", group.Select(item => item.SessionLabel))
                    })
                    .ToList()
            });
        }

        public IActionResult Earnings()
        {
            var doctor = _portalService.GetCurrentDoctor();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var appointments = _portalService.GetAppointmentsForDoctor(doctor.Id);
            var paidAppointments = appointments.Where(appointment => appointment.PaymentCompleted).ToList();
            var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddDays(-1);
            var currentMonthRevenue = paidAppointments
                .Where(appointment => appointment.AppointmentDate >= currentMonthStart)
                .Sum(appointment => appointment.TotalAmount);
            var previousMonthRevenue = paidAppointments
                .Where(appointment => appointment.AppointmentDate >= previousMonthStart && appointment.AppointmentDate <= previousMonthEnd)
                .Sum(appointment => appointment.TotalAmount);
            var revenueTrend = previousMonthRevenue <= 0
                ? (currentMonthRevenue > 0 ? "+100%" : "0%")
                : $"{((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100m:+0;-0;0}%";

            var model = new DoctorEarningsViewModel
            {
                TotalEarningsLabel = PortalFormatting.FormatCurrency(paidAppointments.Sum(appointment => appointment.TotalAmount)),
                ThisMonthLabel = PortalFormatting.FormatCurrency(currentMonthRevenue),
                AveragePerVisitLabel = paidAppointments.Count == 0
                    ? "INR 0"
                    : PortalFormatting.FormatCurrency(paidAppointments.Average(appointment => appointment.TotalAmount)),
                PendingPayoutLabel = PortalFormatting.FormatCurrency(appointments
                    .Where(appointment => !appointment.PaymentCompleted)
                    .Sum(appointment => appointment.TotalAmount)),
                PaidAppointmentsCount = paidAppointments.Count,
                PendingPaymentCount = appointments.Count(appointment => !appointment.PaymentCompleted),
                RevenueTrendLabel = revenueTrend,
                PaymentMethodBreakdown = paidAppointments
                    .GroupBy(appointment => string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "Pending" : appointment.PaymentMethod)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{PortalFormatting.FormatCurrency(group.Sum(item => item.TotalAmount))} collected"
                    })
                    .ToList(),
                StatusBreakdown = appointments
                    .GroupBy(appointment => appointment.Status)
                    .OrderByDescending(group => group.Count())
                    .Select(group => new ReportBreakdownItemViewModel
                    {
                        Label = group.Key,
                        ValueLabel = group.Count().ToString("D2"),
                        Detail = $"{PortalFormatting.FormatCurrency(group.Sum(item => item.TotalAmount))} tied to this status"
                    })
                    .ToList()
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

        private List<AppointmentCardViewModel> BuildAppointmentCards(DemoDoctor doctor, IReadOnlyList<DemoAppointment> appointments)
        {
            var patientsById = _portalService.GetPatients().ToDictionary(patient => patient.Id);

            return appointments
                .Select(appointment =>
                {
                    patientsById.TryGetValue(appointment.PatientId, out var patient);

                    return new AppointmentCardViewModel
                    {
                        Id = appointment.Id,
                        DoctorName = doctor.FullName,
                        DoctorSpecialization = doctor.Specialization,
                        PatientName = patient?.FullName ?? "Unknown patient",
                        DateLabel = PortalFormatting.FormatDate(appointment.AppointmentDate),
                        TimeSlot = appointment.TimeSlot,
                        Status = appointment.Status,
                        FeeLabel = PortalFormatting.FormatCurrency(appointment.TotalAmount),
                        PaymentMethod = string.IsNullOrWhiteSpace(appointment.PaymentMethod) ? "Pending" : appointment.PaymentMethod,
                        PaymentStatus = appointment.PaymentCompleted ? "Paid" : "Pending",
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

        private static decimal ParseCurrency(string value)
        {
            var normalized = value.Replace("INR", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(",", string.Empty).Trim();
            return decimal.TryParse(normalized, out var amount) ? amount : 0m;
        }
    }
}
