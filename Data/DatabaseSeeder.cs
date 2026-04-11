using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Doctor_Appointment_System.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PortalDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Patients.AnyAsync())
        {
            return;
        }

        var passwordHasher = new PasswordHasher<string>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        const string defaultPassword = "12345678";
        const string adminAccessCode = "MEDICORE-ADMIN";

        var patients = new[]
        {
            new PatientEntity
            {
                FullName = "Aarav Patel",
                Email = "aarav.patel@medicore.in",
                PasswordHash = passwordHasher.HashPassword("aarav.patel@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("sneha.sharma@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("karan.mehta@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("ananya.shah@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("rohan.mehta@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("kavya.iyer@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("priya.menon@medicore.in", defaultPassword),
                AccessCode = adminAccessCode,
                AvatarPath = "/images/avatars/doctor-ananya.svg",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            }
        };

        await dbContext.Patients.AddRangeAsync(patients);
        await dbContext.Doctors.AddRangeAsync(doctors);
        await dbContext.Admins.AddRangeAsync(admins);
        await dbContext.SaveChangesAsync();

        var availability = new[]
        {
            new DoctorAvailabilityEntity { DoctorId = doctors[0].Id, DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:00 AM to 01:00 PM", SlotValue = "09:00 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DoctorId = doctors[0].Id, DayLabel = "Mon", SessionLabel = "Morning consultation", TimeRange = "09:00 AM to 01:00 PM", SlotValue = "10:30 AM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DoctorId = doctors[0].Id, DayLabel = "Wed", SessionLabel = "Afternoon consultation", TimeRange = "01:30 PM to 05:30 PM", SlotValue = "02:00 PM", DisplayOrder = 3 },
            new DoctorAvailabilityEntity { DoctorId = doctors[0].Id, DayLabel = "Fri", SessionLabel = "Follow-up session", TimeRange = "03:00 PM to 06:00 PM", SlotValue = "04:00 PM", DisplayOrder = 4 },
            new DoctorAvailabilityEntity { DoctorId = doctors[1].Id, DayLabel = "Tue", SessionLabel = "Neuro review", TimeRange = "11:00 AM to 06:30 PM", SlotValue = "11:30 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DoctorId = doctors[1].Id, DayLabel = "Thu", SessionLabel = "Consultation block", TimeRange = "02:00 PM to 07:00 PM", SlotValue = "03:30 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DoctorId = doctors[1].Id, DayLabel = "Sat", SessionLabel = "Extended visit", TimeRange = "05:00 PM to 07:30 PM", SlotValue = "06:00 PM", DisplayOrder = 3 },
            new DoctorAvailabilityEntity { DoctorId = doctors[2].Id, DayLabel = "Mon", SessionLabel = "Cardiac wellness", TimeRange = "09:00 AM to 12:00 PM", SlotValue = "09:30 AM", DisplayOrder = 1 },
            new DoctorAvailabilityEntity { DoctorId = doctors[2].Id, DayLabel = "Wed", SessionLabel = "Heart check", TimeRange = "12:00 PM to 03:00 PM", SlotValue = "01:00 PM", DisplayOrder = 2 },
            new DoctorAvailabilityEntity { DoctorId = doctors[2].Id, DayLabel = "Fri", SessionLabel = "Evening clinic", TimeRange = "04:00 PM to 06:30 PM", SlotValue = "05:00 PM", DisplayOrder = 3 }
        };

        var doctorRequests = new[]
        {
            new DoctorRequestEntity
            {
                FullName = "Dr. Vikram Rao",
                Email = "vikram.rao@medicore.in",
                PasswordHash = passwordHasher.HashPassword("vikram.rao@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("meera.joshi@medicore.in", defaultPassword),
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
                PasswordHash = passwordHasher.HashPassword("sameer.kulkarni@medicore.in", defaultPassword),
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

        var appointments = new[]
        {
            new AppointmentEntity
            {
                PatientId = patients[0].Id,
                DoctorId = doctors[0].Id,
                AppointmentDate = today.AddDays(2),
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
                PatientId = patients[1].Id,
                DoctorId = doctors[0].Id,
                AppointmentDate = today.AddDays(1),
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
                PatientId = patients[0].Id,
                DoctorId = doctors[1].Id,
                AppointmentDate = today.AddDays(-18),
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
                PatientId = patients[2].Id,
                DoctorId = doctors[0].Id,
                AppointmentDate = today.AddDays(-6),
                TimeSlot = "09:00 AM",
                Status = "Completed",
                ConsultationFee = 800m,
                PlatformFee = 50m,
                PaymentMethod = "Cash at clinic",
                PaymentCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            }
        };

        var notifications = new[]
        {
            new NotificationEntity
            {
                PatientId = patients[0].Id,
                Title = "Appointment reminder",
                Message = $"Upcoming visit with Dr. Ananya Shah on {today.AddDays(2):dd MMM yyyy} at 10:30 AM.",
                Label = "New",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new NotificationEntity
            {
                PatientId = patients[0].Id,
                Title = "Payment received",
                Message = "Your consultation payment for Dr. Rohan Mehta was recorded successfully.",
                Label = "Saved",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        await dbContext.DoctorAvailability.AddRangeAsync(availability);
        await dbContext.DoctorRequests.AddRangeAsync(doctorRequests);
        await dbContext.Appointments.AddRangeAsync(appointments);
        await dbContext.Notifications.AddRangeAsync(notifications);
        await dbContext.SaveChangesAsync();
    }
}
