using Microsoft.EntityFrameworkCore;

namespace Doctor_Appointment_System.Data;

public sealed class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();
    public DbSet<DoctorEntity> Doctors => Set<DoctorEntity>();
    public DbSet<AdminEntity> Admins => Set<AdminEntity>();
    public DbSet<DoctorRequestEntity> DoctorRequests => Set<DoctorRequestEntity>();
    public DbSet<DoctorAvailabilityEntity> DoctorAvailability => Set<DoctorAvailabilityEntity>();
    public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientEntity>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.Email).HasMaxLength(160);
            entity.Property(item => item.Phone).HasMaxLength(40);
            entity.Property(item => item.Address).HasMaxLength(200);
        });

        modelBuilder.Entity<DoctorEntity>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.Email).HasMaxLength(160);
            entity.Property(item => item.Specialization).HasMaxLength(120);
            entity.Property(item => item.HospitalName).HasMaxLength(160);
            entity.Property(item => item.City).HasMaxLength(120);
            entity.Property(item => item.LicenseNumber).HasMaxLength(80);
            entity.Property(item => item.ConsultationFee).HasPrecision(10, 2);
            entity.Property(item => item.Rating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<AdminEntity>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.Email).HasMaxLength(160);
            entity.Property(item => item.AccessCode).HasMaxLength(80);
        });

        modelBuilder.Entity<DoctorRequestEntity>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.Email).HasMaxLength(160);
            entity.Property(item => item.Specialization).HasMaxLength(120);
            entity.Property(item => item.HospitalName).HasMaxLength(160);
            entity.Property(item => item.City).HasMaxLength(120);
            entity.Property(item => item.LicenseNumber).HasMaxLength(80);
            entity.Property(item => item.Status).HasMaxLength(40);
        });

        modelBuilder.Entity<DoctorAvailabilityEntity>(entity =>
        {
            entity.Property(item => item.DayLabel).HasMaxLength(30);
            entity.Property(item => item.SessionLabel).HasMaxLength(120);
            entity.Property(item => item.TimeRange).HasMaxLength(80);
            entity.Property(item => item.SlotValue).HasMaxLength(20);
            entity.HasOne(item => item.Doctor)
                .WithMany(doctor => doctor.AvailabilitySlots)
                .HasForeignKey(item => item.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppointmentEntity>(entity =>
        {
            entity.Property(item => item.TimeSlot).HasMaxLength(20);
            entity.Property(item => item.Status).HasMaxLength(40);
            entity.Property(item => item.PaymentMethod).HasMaxLength(60);
            entity.Property(item => item.ConsultationFee).HasPrecision(10, 2);
            entity.Property(item => item.PlatformFee).HasPrecision(10, 2);
            entity.HasOne(item => item.Patient)
                .WithMany(patient => patient.Appointments)
                .HasForeignKey(item => item.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Doctor)
                .WithMany(doctor => doctor.Appointments)
                .HasForeignKey(item => item.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(120);
            entity.Property(item => item.Label).HasMaxLength(40);
            entity.HasOne(item => item.Patient)
                .WithMany(patient => patient.Notifications)
                .HasForeignKey(item => item.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
