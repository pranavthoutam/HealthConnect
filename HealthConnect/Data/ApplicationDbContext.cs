namespace HealthConnect.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "User");
            });
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });
            builder.Entity<User>(entity => entity.Property(u => u.DateofBirth).IsRequired(false));
            builder.Entity<User>(entity => entity.Property(u => u.ProfilePhoto).IsRequired(false));
            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Doctor>(d => d.UserId);
            builder.Entity<Medicine>()
                .HasOne(m => m.MedicineCategory)
                .WithMany(mc => mc.Medicines)
                .HasForeignKey(m => m.CategoryId);

            // Configure MedicineAlternatives relationships
            builder.Entity<MedicineAlternatives>()
                .HasOne(ma => ma.Medicine)
                .WithMany(m => m.Alternatives)
                .HasForeignKey(ma => ma.MedicineId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<MedicineAlternatives>()
                .HasOne(ma => ma.Alternative)
                .WithMany() // No reverse navigation property
                .HasForeignKey(ma => ma.AlternativeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<Doctor>()
                .HasMany(d => d.Clinics)
                .WithOne(c => c.Doctor)
                .HasForeignKey(c => c.DoctorId);

            builder.Entity<Clinic>()
                .HasMany(c => c.Availabilities)
                .WithOne(a => a.Clinic)
                .HasForeignKey(a => a.ClinicId);
        }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicineCategory> MedicineCategories { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicineAlternatives> MedicinesAlternatives { get;set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
    }
}
