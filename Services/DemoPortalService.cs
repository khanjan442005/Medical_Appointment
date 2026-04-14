using Doctor_Appointment_System.Data;
using Doctor_Appointment_System.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Doctor_Appointment_System.Services;

public sealed class DemoPortalService : IDemoPortalService
{
    private const decimal PlatformFee = 50m;
    private const string RoleSessionKey = "PortalRole";
    private const string UserIdSessionKey = "PortalUserId";
    private const string AdminAccessCode = "MEDICORE-ADMIN";

    private readonly PortalDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public DemoPortalService(PortalDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public DemoPatient GetCurrentPatient()
    {
        var patient = ResolveCurrentPatientEntity()
            ?? throw new InvalidOperationException("A signed-in patient account is required.");

        return MapPatient(patient);
    }

    public DemoDoctor GetCurrentDoctor()
    {
        var doctor = ResolveCurrentDoctorEntity()
            ?? throw new InvalidOperationException("A signed-in doctor account is required.");

        return MapDoctor(doctor);
    }

    public DemoAdmin GetCurrentAdmin()
    {
        var admin = ResolveCurrentAdminEntity()
            ?? throw new InvalidOperationException("A signed-in admin account is required.");

        return MapAdmin(admin);
    }

    public IReadOnlyList<DemoPatient> GetPatients() =>
        _dbContext.Patients.AsNoTracking().OrderBy(item => item.FullName).Select(MapPatientExpression()).ToList();

    public IReadOnlyList<DemoDoctor> GetVerifiedDoctors() =>
        _dbContext.Doctors
            .AsNoTracking()
            .Include(item => item.AvailabilitySlots)
            .Where(item => item.IsVerified)
            .ToList()
            .OrderByDescending(item => item.Rating)
            .ThenBy(item => item.FullName)
            .Select(MapDoctor)
            .ToList();

    public DemoDoctor? GetDoctor(int doctorId)
    {
        var doctor = _dbContext.Doctors
            .AsNoTracking()
            .Include(item => item.AvailabilitySlots)
            .FirstOrDefault(item => item.Id == doctorId && item.IsVerified);

        return doctor is null ? null : MapDoctor(doctor);
    }

    public DemoAppointment? GetAppointment(int appointmentId)
    {
        var appointment = _dbContext.Appointments.AsNoTracking().FirstOrDefault(item => item.Id == appointmentId);
        return appointment is null ? null : MapAppointment(appointment);
    }

    public DemoPatient? GetPatient(int patientId)
    {
        var patient = _dbContext.Patients.AsNoTracking().FirstOrDefault(item => item.Id == patientId);
        return patient is null ? null : MapPatient(patient);
    }

    public IReadOnlyList<DemoAppointment> GetAppointmentsForPatient(int patientId) =>
        _dbContext.Appointments
            .AsNoTracking()
            .Where(item => item.PatientId == patientId)
            .OrderByDescending(item => item.AppointmentDate)
            .ThenByDescending(item => item.CreatedAt)
            .ToList()
            .Select(MapAppointment)
            .ToList();

    public IReadOnlyList<DemoAppointment> GetAppointmentsForDoctor(int doctorId) =>
        _dbContext.Appointments
            .AsNoTracking()
            .Where(item => item.DoctorId == doctorId)
            .OrderBy(item => item.AppointmentDate)
            .ThenBy(item => item.TimeSlot)
            .ToList()
            .Select(MapAppointment)
            .ToList();

    public IReadOnlyList<DemoAppointment> GetAllAppointments() =>
        _dbContext.Appointments
            .AsNoTracking()
            .OrderByDescending(item => item.AppointmentDate)
            .ThenBy(item => item.TimeSlot)
            .ToList()
            .Select(MapAppointment)
            .ToList();

    public IReadOnlyList<DemoNotification> GetNotificationsForPatient(int patientId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.PatientId == patientId)
            .OrderByDescending(item => item.CreatedAt)
            .ToList()
            .Select(MapNotification)
            .ToList();

    public IReadOnlyList<DemoDoctorRequest> GetPendingDoctorRequests() =>
        _dbContext.DoctorRequests
            .AsNoTracking()
            .Where(item => item.Status == "Pending")
            .OrderByDescending(item => item.RequestedAt)
            .ToList()
            .Select(MapDoctorRequest)
            .ToList();

    public IReadOnlyList<DemoDoctorRequest> GetAllDoctorRequests() =>
        _dbContext.DoctorRequests
            .AsNoTracking()
            .OrderByDescending(item => item.RequestedAt)
            .ToList()
            .Select(MapDoctorRequest)
            .ToList();

    public IReadOnlyList<DoctorAvailabilitySlot> GetAvailabilitySlots(int doctorId) =>
        _dbContext.DoctorAvailability
            .AsNoTracking()
            .Where(item => item.DoctorId == doctorId)
            .OrderBy(item => item.DisplayOrder)
            .ToList()
            .GroupBy(item => new { item.DayLabel, item.SessionLabel, item.TimeRange })
            .Select(group => new DoctorAvailabilitySlot
            {
                DayLabel = group.Key.DayLabel,
                SessionLabel = group.Key.SessionLabel,
                TimeRange = group.Key.TimeRange,
                SlotValues = group
                    .OrderBy(item => item.DisplayOrder)
                    .Select(item => item.SlotValue)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            })
            .ToList();

    public DemoAppointment CreatePendingAppointment(int patientId, int doctorId, DateOnly appointmentDate, string timeSlot)
    {
        if (appointmentDate < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new InvalidOperationException("Appointments cannot be booked for a past date.");
        }

        var doctor = _dbContext.Doctors.Include(item => item.AvailabilitySlots).FirstOrDefault(item => item.Id == doctorId && item.IsVerified)
            ?? throw new InvalidOperationException("Doctor not found.");

        var patient = _dbContext.Patients.FirstOrDefault(item => item.Id == patientId)
            ?? throw new InvalidOperationException("Patient not found.");

        var normalizedSlot = timeSlot.Trim();
        var appointmentDayLabel = GetDayLabel(appointmentDate.DayOfWeek);
        var availableSlot = doctor.AvailabilitySlots.Any(item =>
            string.Equals(item.DayLabel, appointmentDayLabel, StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.SlotValue, normalizedSlot, StringComparison.OrdinalIgnoreCase));

        if (!availableSlot)
        {
            throw new InvalidOperationException($"This slot is not available on {appointmentDayLabel}. Choose a slot that matches the doctor's schedule.");
        }

        var slotAlreadyBooked = _dbContext.Appointments.Any(item =>
            item.DoctorId == doctor.Id
            && item.AppointmentDate == appointmentDate
            && item.TimeSlot == normalizedSlot);

        if (slotAlreadyBooked)
        {
            throw new InvalidOperationException("This time slot has already been booked for the selected date.");
        }

        var patientAlreadyBooked = _dbContext.Appointments.Any(item =>
            item.PatientId == patient.Id
            && item.AppointmentDate == appointmentDate
            && item.TimeSlot == normalizedSlot);

        if (patientAlreadyBooked)
        {
            throw new InvalidOperationException("You already have another appointment at this time.");
        }

        var appointment = new AppointmentEntity
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            AppointmentDate = appointmentDate,
            TimeSlot = normalizedSlot,
            Status = "Payment Pending",
            ConsultationFee = doctor.ConsultationFee,
            PlatformFee = PlatformFee,
            PaymentMethod = string.Empty,
            PaymentCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Appointments.Add(appointment);
        _dbContext.Notifications.Add(new NotificationEntity
        {
            PatientId = patient.Id,
            Title = "Booking created",
            Message = $"Booking with {doctor.FullName} is reserved for {PortalFormatting.FormatDate(appointmentDate)} at {timeSlot}. Complete payment to confirm it.",
            Label = "Pending",
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.SaveChanges();

        return MapAppointment(appointment);
    }

    public bool CompleteAppointmentPayment(int appointmentId, string paymentMethod)
    {
        var appointment = _dbContext.Appointments.FirstOrDefault(item => item.Id == appointmentId);
        if (appointment is null)
        {
            return false;
        }

        appointment.PaymentCompleted = true;
        appointment.PaymentMethod = paymentMethod;
        appointment.Status = "Confirmed";

        var doctor = _dbContext.Doctors.First(item => item.Id == appointment.DoctorId);
        _dbContext.Notifications.Add(new NotificationEntity
        {
            PatientId = appointment.PatientId,
            Title = "Booking confirmed",
            Message = $"Payment received. {doctor.FullName} is confirmed for {PortalFormatting.FormatDate(appointment.AppointmentDate)} at {appointment.TimeSlot}.",
            Label = "Saved",
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.SaveChanges();

        return true;
    }

    public DemoDoctorRequest CreateDoctorRequest(string fullName, string email, string specialization, string licenseNumber, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        EnsureEmailAvailable(normalizedEmail);

        var request = new DoctorRequestEntity
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            Specialization = specialization.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            ExperienceYears = 7,
            HospitalName = "CityCare Multi-Speciality",
            City = "Ahmedabad",
            Status = "Pending",
            AvatarPath = "/images/avatars/doctor-vikram.svg",
            RequestedAt = DateTime.UtcNow
        };

        _dbContext.DoctorRequests.Add(request);
        _dbContext.SaveChanges();
        return MapDoctorRequest(request);
    }

    public DemoPatient CreatePatient(string fullName, string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);
        EnsureEmailAvailable(normalizedEmail);

        var patient = new PatientEntity
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            Phone = "+91 90000 00000",
            Address = "Ahmedabad, Gujarat",
            DateOfBirth = null,
            AvatarPath = "/images/avatars/patient-aarav.svg",
            StatusLabel = "Active patient",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Patients.Add(patient);
        _dbContext.SaveChanges();
        return MapPatient(patient);
    }

    public DemoAdmin CreateAdmin(string fullName, string email, string password, string accessCode)
    {
        if (!string.Equals(accessCode?.Trim(), AdminAccessCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid admin access code.");
        }

        var normalizedEmail = NormalizeEmail(email);
        EnsureEmailAvailable(normalizedEmail);

        var admin = new AdminEntity
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            AccessCode = AdminAccessCode,
            AvatarPath = "/images/avatars/doctor-ananya.svg",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Admins.Add(admin);
        _dbContext.SaveChanges();
        return MapAdmin(admin);
    }

    public DemoDoctor CreateVerifiedDoctor(string fullName, string email, string specialization, string licenseNumber, string password, int experienceYears, string hospitalName, string city, decimal consultationFee)
    {
        var normalizedEmail = NormalizeEmail(email);
        EnsureEmailAvailable(normalizedEmail);

        var doctor = new DoctorEntity
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(normalizedEmail, password),
            Specialization = specialization.Trim(),
            ExperienceYears = experienceYears,
            HospitalName = hospitalName.Trim(),
            City = city.Trim(),
            ConsultationFee = consultationFee,
            Rating = 4.8m,
            IsVerified = true,
            LicenseNumber = licenseNumber.Trim(),
            Bio = "Admin-added doctor profile with verified access to the MediCore appointment workflow.",
            Languages = "English, Hindi",
            AvatarPath = "/images/avatars/doctor-vikram.svg",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Doctors.Add(doctor);
        _dbContext.SaveChanges();

        _dbContext.DoctorAvailability.AddRange(
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:00 AM to 12:00 PM", SlotValue = "09:30 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Wed", SessionLabel = "Afternoon consultation", TimeRange = "01:00 PM to 04:00 PM", SlotValue = "02:00 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Fri", SessionLabel = "Evening consultation", TimeRange = "05:00 PM to 07:00 PM", SlotValue = "05:30 PM", DisplayOrder = 3 });
        _dbContext.SaveChanges();

        return MapDoctor(_dbContext.Doctors.Include(item => item.AvailabilitySlots).First(item => item.Id == doctor.Id));
    }

    public (bool Success, string ErrorMessage) DeleteDoctor(int doctorId)
    {
        var doctor = _dbContext.Doctors
            .Include(item => item.Appointments)
            .Include(item => item.AvailabilitySlots)
            .FirstOrDefault(item => item.Id == doctorId);

        if (doctor is null)
        {
            return (false, "Doctor not found.");
        }

        if (doctor.Appointments.Any())
        {
            return (false, "This doctor has appointment records and cannot be deleted.");
        }

        _dbContext.DoctorAvailability.RemoveRange(doctor.AvailabilitySlots);
        _dbContext.Doctors.Remove(doctor);
        _dbContext.SaveChanges();

        return (true, string.Empty);
    }

    public bool ApproveDoctorRequest(int requestId)
    {
        var request = _dbContext.DoctorRequests.FirstOrDefault(item => item.Id == requestId && item.Status == "Pending");
        if (request is null)
        {
            return false;
        }

        if (_dbContext.Doctors.Any(item => item.Email == request.Email))
        {
            request.Status = "Approved";
            _dbContext.SaveChanges();
            return true;
        }

        var doctor = new DoctorEntity
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            Specialization = request.Specialization,
            ExperienceYears = request.ExperienceYears,
            HospitalName = request.HospitalName,
            City = request.City,
            ConsultationFee = 900m,
            Rating = 4.6m,
            IsVerified = true,
            LicenseNumber = request.LicenseNumber,
            Bio = "Recently onboarded specialist approved by the MediCore admin workflow.",
            Languages = "English, Hindi",
            AvatarPath = request.AvatarPath,
            CreatedAt = DateTime.UtcNow
        };

        request.Status = "Approved";
        _dbContext.Doctors.Add(doctor);
        _dbContext.SaveChanges();

        _dbContext.DoctorAvailability.AddRange(
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:30 AM to 12:00 PM", SlotValue = "10:00 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Wed", SessionLabel = "Afternoon follow-up", TimeRange = "01:00 PM to 03:30 PM", SlotValue = "01:30 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DoctorId = doctor.Id, DayLabel = "Fri", SessionLabel = "Evening clinic", TimeRange = "05:00 PM to 07:00 PM", SlotValue = "05:30 PM", DisplayOrder = 3 });

        _dbContext.SaveChanges();
        return true;
    }

    public bool RejectDoctorRequest(int requestId)
    {
        var request = _dbContext.DoctorRequests.FirstOrDefault(item => item.Id == requestId && item.Status == "Pending");
        if (request is null)
        {
            return false;
        }

        request.Status = "Rejected";
        _dbContext.SaveChanges();
        return true;
    }

    public PortalMetrics GetPortalMetrics()
    {
        var appointments = _dbContext.Appointments.AsNoTracking().ToList();
        var doctorRequests = _dbContext.DoctorRequests.AsNoTracking().ToList();
        var paidAppointments = appointments.Where(item => item.PaymentCompleted).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart.AddDays(-1);
        var currentMonthRevenue = paidAppointments.Where(item => item.AppointmentDate >= currentMonthStart).Sum(item => item.ConsultationFee + item.PlatformFee);
        var previousMonthRevenue = paidAppointments.Where(item => item.AppointmentDate >= previousMonthStart && item.AppointmentDate <= previousMonthEnd).Sum(item => item.ConsultationFee + item.PlatformFee);
        var reviewedRequests = doctorRequests.Count(item => item.Status != "Pending");
        var approvedRequests = doctorRequests.Count(item => item.Status == "Approved");
        var revenueTrend = previousMonthRevenue <= 0
            ? (currentMonthRevenue > 0 ? "+100%" : "0%")
            : $"{((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100m:+0;-0;0}%";

        return new PortalMetrics
        {
            TotalDoctors = _dbContext.Doctors.AsNoTracking().Count(item => item.IsVerified),
            TotalPatients = _dbContext.Patients.AsNoTracking().Count(),
            TotalAppointments = appointments.Count,
            PendingDoctorVerifications = doctorRequests.Count(item => item.Status == "Pending"),
            PendingPayments = appointments.Count(item => item.Status == "Payment Pending"),
            ActiveDoctors = _dbContext.Doctors.AsNoTracking().Count(item => item.IsVerified),
            WeeklyAppointments = appointments.Count(item => item.AppointmentDate >= today.AddDays(-3) && item.AppointmentDate <= today.AddDays(7)),
            NewRegistrations = _dbContext.Patients.AsNoTracking().Count(item => item.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                + doctorRequests.Count(item => item.RequestedAt >= DateTime.UtcNow.AddDays(-30)),
            ApprovalRateLabel = reviewedRequests == 0 ? "100%" : $"{Math.Round((decimal)approvedRequests / reviewedRequests * 100m):0}%",
            RevenueLabel = PortalFormatting.FormatCurrency(paidAppointments.Sum(item => item.ConsultationFee + item.PlatformFee)),
            RevenueTrendLabel = revenueTrend
        };
    }

    public async Task<(bool Success, string ErrorMessage)> TryLoginAsync(string role, string email, string password)
    {
        var normalizedEmail = NormalizeEmail(email);

        switch (role?.Trim())
        {
            case "Patient":
            {
                var patient = _dbContext.Patients.AsNoTracking().FirstOrDefault(item => item.Email == normalizedEmail);
                if (patient is null || !IsPasswordValid(normalizedEmail, patient.PasswordHash, password))
                {
                    return (false, "Invalid patient login details.");
                }

                await SignInAsync("Patient", patient.Id, patient.FullName, patient.Email);
                return (true, string.Empty);
            }

            case "Doctor":
            {
                var doctor = _dbContext.Doctors.AsNoTracking().FirstOrDefault(item => item.Email == normalizedEmail && item.IsVerified);
                if (doctor is not null)
                {
                    if (!IsPasswordValid(normalizedEmail, doctor.PasswordHash, password))
                    {
                        return (false, "Invalid doctor login details.");
                    }

                    await SignInAsync("Doctor", doctor.Id, doctor.FullName, doctor.Email);
                    return (true, string.Empty);
                }

                var request = _dbContext.DoctorRequests.FirstOrDefault(item => item.Email == normalizedEmail);
                if (request is null || !IsPasswordValid(normalizedEmail, request.PasswordHash, password))
                {
                    return (false, "Invalid doctor login details.");
                }

                if (string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "This doctor registration request has been rejected.");
                }

                if (string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    var approved = ApproveDoctorRequest(request.Id);
                    if (!approved)
                    {
                        return (false, "This doctor account is still pending admin verification.");
                    }
                }

                doctor = _dbContext.Doctors.AsNoTracking().FirstOrDefault(item => item.Email == normalizedEmail && item.IsVerified);
                if (doctor is null)
                {
                    return (false, "Doctor account could not be activated. Please contact admin.");
                }

                await SignInAsync("Doctor", doctor.Id, doctor.FullName, doctor.Email);
                return (true, string.Empty);
            }

            case "Admin":
            {
                var admin = _dbContext.Admins.AsNoTracking().FirstOrDefault(item => item.Email == normalizedEmail);
                if (admin is null || !IsPasswordValid(normalizedEmail, admin.PasswordHash, password))
                {
                    return (false, "Invalid admin login details.");
                }

                await SignInAsync("Admin", admin.Id, admin.FullName, admin.Email);
                return (true, string.Empty);
            }

            default:
                return (false, "Invalid role selected.");
        }
    }

    public async Task LogoutAsync()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Clear();
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public bool TryBeginPasswordReset(string email, out string normalizedEmail, out string errorMessage)
    {
        normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            errorMessage = "Email is required.";
            return false;
        }

        var lookupEmail = normalizedEmail;
        var exists = _dbContext.Patients.AsNoTracking().Any(item => item.Email == lookupEmail)
            || _dbContext.Doctors.AsNoTracking().Any(item => item.Email == lookupEmail)
            || _dbContext.Admins.AsNoTracking().Any(item => item.Email == lookupEmail)
            || _dbContext.DoctorRequests.AsNoTracking().Any(item => item.Email == lookupEmail);

        errorMessage = exists ? string.Empty : "This email address was not found in the system.";
        return exists;
    }

    public bool ResetPassword(string email, string newPassword, out string errorMessage)
    {
        errorMessage = string.Empty;
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(newPassword))
        {
            errorMessage = "Email and new password are required.";
            return false;
        }

        var patient = _dbContext.Patients.FirstOrDefault(item => item.Email == normalizedEmail);
        if (patient is not null)
        {
            patient.PasswordHash = _passwordHasher.HashPassword(normalizedEmail, newPassword);
            _dbContext.SaveChanges();
            return true;
        }

        var doctor = _dbContext.Doctors.FirstOrDefault(item => item.Email == normalizedEmail);
        if (doctor is not null)
        {
            doctor.PasswordHash = _passwordHasher.HashPassword(normalizedEmail, newPassword);
            _dbContext.SaveChanges();
            return true;
        }

        var admin = _dbContext.Admins.FirstOrDefault(item => item.Email == normalizedEmail);
        if (admin is not null)
        {
            admin.PasswordHash = _passwordHasher.HashPassword(normalizedEmail, newPassword);
            _dbContext.SaveChanges();
            return true;
        }

        var request = _dbContext.DoctorRequests.FirstOrDefault(item => item.Email == normalizedEmail);
        if (request is not null)
        {
            request.PasswordHash = _passwordHasher.HashPassword(normalizedEmail, newPassword);
            _dbContext.SaveChanges();
            return true;
        }

        errorMessage = "The email address was not found for password reset.";
        return false;
    }

    private static Func<PatientEntity, DemoPatient> MapPatientExpression() => patient => new DemoPatient
    {
        Id = patient.Id,
        FullName = patient.FullName,
        Email = patient.Email,
        Phone = patient.Phone,
        Address = patient.Address,
        DateOfBirthLabel = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.ToString("dd MMM yyyy") : "Not added yet",
        AvatarPath = patient.AvatarPath,
        StatusLabel = patient.StatusLabel
    };

    private DemoPatient MapPatient(PatientEntity patient) => MapPatientExpression()(patient);

    private DemoDoctor MapDoctor(DoctorEntity doctor) => new()
    {
        Id = doctor.Id,
        FullName = doctor.FullName,
        Email = doctor.Email,
        Specialization = doctor.Specialization,
        ExperienceYears = doctor.ExperienceYears,
        HospitalName = doctor.HospitalName,
        City = doctor.City,
        ConsultationFee = doctor.ConsultationFee,
        Rating = doctor.Rating,
        IsVerified = doctor.IsVerified,
        LicenseNumber = doctor.LicenseNumber,
        Bio = doctor.Bio,
        Languages = doctor.Languages,
        AvatarPath = doctor.AvatarPath,
        AvailableSlots = doctor.AvailabilitySlots.OrderBy(item => item.DisplayOrder).Select(item => item.SlotValue).Distinct().ToList()
    };

    private DemoAdmin MapAdmin(AdminEntity admin) => new()
    {
        FullName = admin.FullName,
        Email = admin.Email
    };

    private static DemoAppointment MapAppointment(AppointmentEntity appointment) => new()
    {
        Id = appointment.Id,
        PatientId = appointment.PatientId,
        DoctorId = appointment.DoctorId,
        AppointmentDate = appointment.AppointmentDate,
        TimeSlot = appointment.TimeSlot,
        Status = appointment.Status,
        ConsultationFee = appointment.ConsultationFee,
        PlatformFee = appointment.PlatformFee,
        PaymentMethod = appointment.PaymentMethod,
        PaymentCompleted = appointment.PaymentCompleted,
        CreatedAt = appointment.CreatedAt
    };

    private static DemoNotification MapNotification(NotificationEntity notification) => new()
    {
        Id = notification.Id,
        PatientId = notification.PatientId,
        Title = notification.Title,
        Message = notification.Message,
        Label = notification.Label,
        CreatedAt = notification.CreatedAt
    };

    private static DemoDoctorRequest MapDoctorRequest(DoctorRequestEntity request) => new()
    {
        Id = request.Id,
        FullName = request.FullName,
        Email = request.Email,
        Specialization = request.Specialization,
        LicenseNumber = request.LicenseNumber,
        ExperienceYears = request.ExperienceYears,
        HospitalName = request.HospitalName,
        City = request.City,
        Status = request.Status,
        AvatarPath = request.AvatarPath,
        RequestedAt = request.RequestedAt
    };

    private PatientEntity? ResolveCurrentPatientEntity()
    {
        var (role, userId) = GetSession();
        if (role == "Patient" && userId.HasValue)
        {
            return _dbContext.Patients.AsNoTracking().FirstOrDefault(item => item.Id == userId.Value);
        }

        return null;
    }

    private DoctorEntity? ResolveCurrentDoctorEntity()
    {
        var (role, userId) = GetSession();
        if (role == "Doctor" && userId.HasValue)
        {
            return _dbContext.Doctors.AsNoTracking().Include(item => item.AvailabilitySlots).FirstOrDefault(item => item.Id == userId.Value && item.IsVerified);
        }

        return null;
    }

    private AdminEntity? ResolveCurrentAdminEntity()
    {
        var (role, userId) = GetSession();
        if (role == "Admin" && userId.HasValue)
        {
            return _dbContext.Admins.AsNoTracking().FirstOrDefault(item => item.Id == userId.Value);
        }

        return null;
    }

    private (string? Role, int? UserId) GetSession()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
                return (roleClaim, parsedUserId);
            }
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return (null, null);
        }

        var role = session.GetString(RoleSessionKey);
        var userId = session.GetInt32(UserIdSessionKey);
        return (role, userId);
    }

    private void SetSession(string role, int userId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return;
        }

        session.SetString(RoleSessionKey, role);
        session.SetInt32(UserIdSessionKey, userId);
    }

    private async Task SignInAsync(string role, int userId, string fullName, string email)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("The current HTTP context is not available.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        SetSession(role, userId);
    }

    private void EnsureEmailAvailable(string normalizedEmail)
    {
        if (_dbContext.Patients.Any(item => item.Email == normalizedEmail)
            || _dbContext.Doctors.Any(item => item.Email == normalizedEmail)
            || _dbContext.Admins.Any(item => item.Email == normalizedEmail)
            || _dbContext.DoctorRequests.Any(item => item.Email == normalizedEmail))
        {
            throw new InvalidOperationException("This email address is already in use.");
        }
    }

    private bool IsPasswordValid(string email, string hash, string password) =>
        _passwordHasher.VerifyHashedPassword(email, hash, password) != PasswordVerificationResult.Failed;

    private static string GetDayLabel(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => "Mon",
        DayOfWeek.Tuesday => "Tue",
        DayOfWeek.Wednesday => "Wed",
        DayOfWeek.Thursday => "Thu",
        DayOfWeek.Friday => "Fri",
        DayOfWeek.Saturday => "Sat",
        DayOfWeek.Sunday => "Sun",
        _ => string.Empty
    };

    private static string NormalizeEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}
