using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Doctor_Appointment_System.Data;

public static class DatabaseSeeder
{
    private const string DemoPassword = "MediCore@123";
    private const string AdminAccessCode = "MEDICORE-ADMIN";

    public static async Task SeedAsync(PortalDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        var passwordHasher = new PasswordHasher<string>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var patients = new[]
        {
            new PatientEntity
            {
                FullName = "Aarav Patel",
                Email = "aarav.patel@medicore.in",
                Phone = "+91 98765 43210",
                Address = "Satellite, Ahmedabad",
                DateOfBirth = new DateOnly(1995, 1, 12),
                AvatarPath = "/images/avatars/patient-aarav.svg",
                StatusLabel = "Active patient",
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new PatientEntity
            {
                FullName = "Sneha Sharma",
                Email = "sneha.sharma@medicore.in",
                Phone = "+91 98710 12345",
                Address = "Navrangpura, Ahmedabad",
                DateOfBirth = new DateOnly(1993, 4, 3),
                AvatarPath = "/images/avatars/patient-aarav.svg",
                StatusLabel = "Active patient",
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new PatientEntity
            {
                FullName = "Karan Mehta",
                Email = "karan.mehta@medicore.in",
                Phone = "+91 98111 22334",
                Address = "Prahlad Nagar, Ahmedabad",
                DateOfBirth = new DateOnly(1991, 11, 26),
                AvatarPath = "/images/avatars/patient-aarav.svg",
                StatusLabel = "Active patient",
                CreatedAt = DateTime.UtcNow.AddDays(-19)
            }
        };

        var doctors = new[]
        {
            new DoctorEntity
            {
                FullName = "Dr. Ananya Shah",
                Email = "ananya.shah@medicore.in",
                Specialization = "Pediatrician",
                ExperienceYears = 10,
                HospitalName = "Arogya Child Care Clinic",
                City = "Ahmedabad",
                ConsultationFee = 800m,
                Rating = 4.9m,
                IsVerified = true,
                LicenseNumber = "GMC-2014-1842",
                Bio = "Focused on preventive child healthcare, recurring wellness plans, and parent-first consultation support.",
                Languages = "English, Hindi, Gujarati",
                AvatarPath = "/images/avatars/doctor-ananya.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new DoctorEntity
            {
                FullName = "Dr. Rohan Mehta",
                Email = "rohan.mehta@medicore.in",
                Specialization = "Neurologist",
                ExperienceYears = 12,
                HospitalName = "NeuroCare Institute",
                City = "Mumbai",
                ConsultationFee = 1100m,
                Rating = 4.7m,
                IsVerified = true,
                LicenseNumber = "MMC-2011-5631",
                Bio = "Known for detailed diagnosis, structured follow-up plans, and efficient consultation reviews.",
                Languages = "English, Hindi, Marathi",
                AvatarPath = "/images/avatars/doctor-rohan.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new DoctorEntity
            {
                FullName = "Dr. Kavya Iyer",
                Email = "kavya.iyer@medicore.in",
                Specialization = "Cardiologist",
                ExperienceYears = 9,
                HospitalName = "Pulse Heart Centre",
                City = "Bengaluru",
                ConsultationFee = 950m,
                Rating = 4.8m,
                IsVerified = true,
                LicenseNumber = "KMC-2015-4092",
                Bio = "Delivers preventive cardiac care with a strong focus on long-term patient monitoring.",
                Languages = "English, Hindi, Tamil",
                AvatarPath = "/images/avatars/doctor-vikram.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-43)
            }
        };

        var admins = new[]
        {
            new AdminEntity
            {
                FullName = "Priya Menon",
                Email = "priya.menon@medicore.in",
                AccessCode = AdminAccessCode,
                AvatarPath = "/images/avatars/doctor-ananya.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            }
        };

        var doctorRequests = new[]
        {
            new DoctorRequestEntity
            {
                FullName = "Dr. Vikram Rao",
                Email = "vikram.rao@medicore.in",
                Specialization = "Cardiologist",
                LicenseNumber = "MMC-2012-7321",
                ExperienceYears = 12,
                HospitalName = "Sanjeevani Heart Centre",
                City = "Pune",
                Status = "Pending",
                AvatarPath = "/images/avatars/doctor-vikram.svg",
                RequestedAt = DateTime.UtcNow.AddDays(-1)
            },
            new DoctorRequestEntity
            {
                FullName = "Dr. Meera Joshi",
                Email = "meera.joshi@medicore.in",
                Specialization = "Dermatologist",
                LicenseNumber = "MMC-2013-2449",
                ExperienceYears = 8,
                HospitalName = "Skin First Clinic",
                City = "Surat",
                Status = "Approved",
                AvatarPath = "/images/avatars/doctor-ananya.svg",
                RequestedAt = DateTime.UtcNow.AddDays(-18)
            },
            new DoctorRequestEntity
            {
                FullName = "Dr. Sameer Kulkarni",
                Email = "sameer.kulkarni@medicore.in",
                Specialization = "Orthopedic Surgeon",
                LicenseNumber = "MMC-2010-3135",
                ExperienceYears = 14,
                HospitalName = "OrthoLife Centre",
                City = "Nashik",
                Status = "Rejected",
                AvatarPath = "/images/avatars/doctor-rohan.svg",
                RequestedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        foreach (var patient in patients)
        {
            patient.PasswordHash = passwordHasher.HashPassword(patient.Email, DemoPassword);
            await UpsertPatientAsync(dbContext, patient);
        }

        foreach (var doctor in doctors)
        {
            doctor.PasswordHash = passwordHasher.HashPassword(doctor.Email, DemoPassword);
            await UpsertDoctorAsync(dbContext, doctor);
        }

        foreach (var admin in admins)
        {
            admin.PasswordHash = passwordHasher.HashPassword(admin.Email, DemoPassword);
            await UpsertAdminAsync(dbContext, admin);
        }

        foreach (var request in doctorRequests)
        {
            request.PasswordHash = passwordHasher.HashPassword(request.Email, DemoPassword);
            await UpsertDoctorRequestAsync(dbContext, request);
        }

        await dbContext.SaveChangesAsync();

        var patientIds = await dbContext.Patients
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Email, item => item.Id);
        var doctorIds = await dbContext.Doctors
            .AsNoTracking()
            .Where(item => item.IsVerified)
            .ToDictionaryAsync(item => item.Email, item => item.Id);

        await EnsureAvailabilityAsync(dbContext, doctorIds["ananya.shah@medicore.in"], new[]
        {
            new DoctorAvailabilityEntity { DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:00 AM to 01:00 PM", SlotValue = "09:00 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:00 AM to 01:00 PM", SlotValue = "10:30 AM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DayLabel = "Wed", SessionLabel = "Afternoon consultation", TimeRange = "01:30 PM to 05:30 PM", SlotValue = "02:00 PM", DisplayOrder = 3 },
            new DoctorAvailabilityEntity { DayLabel = "Fri", SessionLabel = "Follow-up session", TimeRange = "03:00 PM to 06:00 PM", SlotValue = "04:00 PM", DisplayOrder = 4 }
        });

        await EnsureAvailabilityAsync(dbContext, doctorIds["rohan.mehta@medicore.in"], new[]
        {
            new DoctorAvailabilityEntity { DayLabel = "Tue", SessionLabel = "Neuro review", TimeRange = "11:00 AM to 06:30 PM", SlotValue = "11:30 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DayLabel = "Thu", SessionLabel = "Consultation block", TimeRange = "02:00 PM to 07:00 PM", SlotValue = "03:30 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DayLabel = "Sat", SessionLabel = "Extended visit", TimeRange = "05:00 PM to 07:30 PM", SlotValue = "06:00 PM", DisplayOrder = 3 }
        });

        await EnsureAvailabilityAsync(dbContext, doctorIds["kavya.iyer@medicore.in"], new[]
        {
            new DoctorAvailabilityEntity { DayLabel = "Mon", SessionLabel = "Cardiac wellness", TimeRange = "09:00 AM to 12:00 PM", SlotValue = "09:30 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DayLabel = "Wed", SessionLabel = "Heart check", TimeRange = "12:00 PM to 03:00 PM", SlotValue = "01:00 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DayLabel = "Fri", SessionLabel = "Evening clinic", TimeRange = "04:00 PM to 06:30 PM", SlotValue = "05:00 PM", DisplayOrder = 3 }
        });

        if (!await dbContext.Appointments.AnyAsync())
        {
            var confirmedDate = GetUpcomingDate(today, DayOfWeek.Monday);
            var paymentPendingDate = GetUpcomingDate(today, DayOfWeek.Wednesday);
            var completedRohanDate = GetPreviousDate(today, DayOfWeek.Tuesday, 2);
            var completedAnanyaDate = GetPreviousDate(today, DayOfWeek.Monday, 1);

            await dbContext.Appointments.AddRangeAsync(
                new AppointmentEntity
                {
                    PatientId = patientIds["aarav.patel@medicore.in"],
                    DoctorId = doctorIds["ananya.shah@medicore.in"],
                    AppointmentDate = confirmedDate,
                    TimeSlot = "10:30 AM",
                    Status = "Confirmed",
                    ConsultationFee = 800m,
                    PlatformFee = 50m,
                    PaymentMethod = "UPI",
                    PaymentCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new AppointmentEntity
                {
                    PatientId = patientIds["sneha.sharma@medicore.in"],
                    DoctorId = doctorIds["ananya.shah@medicore.in"],
                    AppointmentDate = paymentPendingDate,
                    TimeSlot = "02:00 PM",
                    Status = "Payment Pending",
                    ConsultationFee = 800m,
                    PlatformFee = 50m,
                    PaymentMethod = string.Empty,
                    PaymentCompleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new AppointmentEntity
                {
                    PatientId = patientIds["aarav.patel@medicore.in"],
                    DoctorId = doctorIds["rohan.mehta@medicore.in"],
                    AppointmentDate = completedRohanDate,
                    TimeSlot = "11:30 AM",
                    Status = "Completed",
                    ConsultationFee = 1100m,
                    PlatformFee = 50m,
                    PaymentMethod = "Card",
                    PaymentCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-22)
                },
                new AppointmentEntity
                {
                    PatientId = patientIds["karan.mehta@medicore.in"],
                    DoctorId = doctorIds["ananya.shah@medicore.in"],
                    AppointmentDate = completedAnanyaDate,
                    TimeSlot = "09:00 AM",
                    Status = "Completed",
                    ConsultationFee = 800m,
                    PlatformFee = 50m,
                    PaymentMethod = "Cash at clinic",
                    PaymentCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                });
        }

        if (!await dbContext.Notifications.AnyAsync())
        {
            var reminderDate = GetUpcomingDate(today, DayOfWeek.Monday);

            await dbContext.Notifications.AddRangeAsync(
                new NotificationEntity
                {
                    PatientId = patientIds["aarav.patel@medicore.in"],
                    Title = "Appointment reminder",
                    Message = $"Upcoming visit with Dr. Ananya Shah on {reminderDate:dd MMM yyyy} at 10:30 AM.",
                    Label = "New",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new NotificationEntity
                {
                    PatientId = patientIds["aarav.patel@medicore.in"],
                    Title = "Payment received",
                    Message = "Your consultation payment for Dr. Rohan Mehta was recorded successfully.",
                    Label = "Saved",
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task UpsertPatientAsync(PortalDbContext dbContext, PatientEntity seed)
    {
        var existing = await dbContext.Patients.FirstOrDefaultAsync(item => item.Email == seed.Email);
        if (existing is null)
        {
            dbContext.Patients.Add(seed);
            return;
        }

        existing.PasswordHash = seed.PasswordHash;
        existing.FullName = string.IsNullOrWhiteSpace(existing.FullName) ? seed.FullName : existing.FullName;
        existing.Phone = string.IsNullOrWhiteSpace(existing.Phone) ? seed.Phone : existing.Phone;
        existing.Address = string.IsNullOrWhiteSpace(existing.Address) ? seed.Address : existing.Address;
        existing.DateOfBirth ??= seed.DateOfBirth;
        existing.AvatarPath = string.IsNullOrWhiteSpace(existing.AvatarPath) ? seed.AvatarPath : existing.AvatarPath;
        existing.StatusLabel = string.IsNullOrWhiteSpace(existing.StatusLabel) ? seed.StatusLabel : existing.StatusLabel;
        if (existing.CreatedAt == default)
        {
            existing.CreatedAt = seed.CreatedAt;
        }
    }

    private static async Task UpsertDoctorAsync(PortalDbContext dbContext, DoctorEntity seed)
    {
        var existing = await dbContext.Doctors.FirstOrDefaultAsync(item => item.Email == seed.Email);
        if (existing is null)
        {
            dbContext.Doctors.Add(seed);
            return;
        }

        existing.PasswordHash = seed.PasswordHash;
        existing.FullName = string.IsNullOrWhiteSpace(existing.FullName) ? seed.FullName : existing.FullName;
        existing.Specialization = string.IsNullOrWhiteSpace(existing.Specialization) ? seed.Specialization : existing.Specialization;
        existing.ExperienceYears = existing.ExperienceYears <= 0 ? seed.ExperienceYears : existing.ExperienceYears;
        existing.HospitalName = string.IsNullOrWhiteSpace(existing.HospitalName) ? seed.HospitalName : existing.HospitalName;
        existing.City = string.IsNullOrWhiteSpace(existing.City) ? seed.City : existing.City;
        existing.ConsultationFee = existing.ConsultationFee <= 0 ? seed.ConsultationFee : existing.ConsultationFee;
        existing.Rating = existing.Rating <= 0 ? seed.Rating : existing.Rating;
        existing.IsVerified = existing.IsVerified || seed.IsVerified;
        existing.LicenseNumber = string.IsNullOrWhiteSpace(existing.LicenseNumber) ? seed.LicenseNumber : existing.LicenseNumber;
        existing.Bio = string.IsNullOrWhiteSpace(existing.Bio) ? seed.Bio : existing.Bio;
        existing.Languages = string.IsNullOrWhiteSpace(existing.Languages) ? seed.Languages : existing.Languages;
        existing.AvatarPath = string.IsNullOrWhiteSpace(existing.AvatarPath) ? seed.AvatarPath : existing.AvatarPath;
        if (existing.CreatedAt == default)
        {
            existing.CreatedAt = seed.CreatedAt;
        }
    }

    private static async Task UpsertAdminAsync(PortalDbContext dbContext, AdminEntity seed)
    {
        var existing = await dbContext.Admins.FirstOrDefaultAsync(item => item.Email == seed.Email);
        if (existing is null)
        {
            dbContext.Admins.Add(seed);
            return;
        }

        existing.PasswordHash = seed.PasswordHash;
        existing.FullName = string.IsNullOrWhiteSpace(existing.FullName) ? seed.FullName : existing.FullName;
        existing.AccessCode = string.IsNullOrWhiteSpace(existing.AccessCode) ? seed.AccessCode : existing.AccessCode;
        existing.AvatarPath = string.IsNullOrWhiteSpace(existing.AvatarPath) ? seed.AvatarPath : existing.AvatarPath;
        if (existing.CreatedAt == default)
        {
            existing.CreatedAt = seed.CreatedAt;
        }
    }

    private static async Task UpsertDoctorRequestAsync(PortalDbContext dbContext, DoctorRequestEntity seed)
    {
        var existing = await dbContext.DoctorRequests.FirstOrDefaultAsync(item => item.Email == seed.Email);
        if (existing is null)
        {
            dbContext.DoctorRequests.Add(seed);
            return;
        }

        existing.PasswordHash = seed.PasswordHash;
        existing.FullName = string.IsNullOrWhiteSpace(existing.FullName) ? seed.FullName : existing.FullName;
        existing.Specialization = string.IsNullOrWhiteSpace(existing.Specialization) ? seed.Specialization : existing.Specialization;
        existing.LicenseNumber = string.IsNullOrWhiteSpace(existing.LicenseNumber) ? seed.LicenseNumber : existing.LicenseNumber;
        existing.ExperienceYears = existing.ExperienceYears <= 0 ? seed.ExperienceYears : existing.ExperienceYears;
        existing.HospitalName = string.IsNullOrWhiteSpace(existing.HospitalName) ? seed.HospitalName : existing.HospitalName;
        existing.City = string.IsNullOrWhiteSpace(existing.City) ? seed.City : existing.City;
        existing.Status = string.IsNullOrWhiteSpace(existing.Status) ? seed.Status : existing.Status;
        existing.AvatarPath = string.IsNullOrWhiteSpace(existing.AvatarPath) ? seed.AvatarPath : existing.AvatarPath;
        if (existing.RequestedAt == default)
        {
            existing.RequestedAt = seed.RequestedAt;
        }
    }

    private static async Task EnsureAvailabilityAsync(PortalDbContext dbContext, int doctorId, IEnumerable<DoctorAvailabilityEntity> seeds)
    {
        var existingSlots = await dbContext.DoctorAvailability
            .Where(item => item.DoctorId == doctorId)
            .ToListAsync();

        foreach (var seed in seeds)
        {
            var existing = existingSlots.FirstOrDefault(item =>
                string.Equals(item.DayLabel, seed.DayLabel, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.SlotValue, seed.SlotValue, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                seed.DoctorId = doctorId;
                dbContext.DoctorAvailability.Add(seed);
                continue;
            }

            existing.SessionLabel = seed.SessionLabel;
            existing.TimeRange = seed.TimeRange;
            existing.DisplayOrder = seed.DisplayOrder;
        }
    }

    private static DateOnly GetUpcomingDate(DateOnly today, DayOfWeek targetDay)
    {
        var daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
        {
            daysUntilTarget = 7;
        }

        return today.AddDays(daysUntilTarget);
    }

    private static DateOnly GetPreviousDate(DateOnly today, DayOfWeek targetDay, int additionalWeeks = 0)
    {
        var daysSinceTarget = ((int)today.DayOfWeek - (int)targetDay + 7) % 7;
        if (daysSinceTarget == 0)
        {
            daysSinceTarget = 7;
        }

        return today.AddDays(-(daysSinceTarget + (additionalWeeks * 7)));
    }
}
