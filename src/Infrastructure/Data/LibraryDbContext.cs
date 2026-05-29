using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Infrastructure.Data;

// ─────────────────────────────────────────────────────────────
// DB CONTEXT
// ─────────────────────────────────────────────────────────────
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

    public DbSet<User>         Users         => Set<User>();
    public DbSet<Author>       Authors       => Set<Author>();
    public DbSet<Book>         Books         => Set<Book>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfiguration(new UserConfig());
        mb.ApplyConfiguration(new AuthorConfig());
        mb.ApplyConfiguration(new BookConfig());
        mb.ApplyConfiguration(new BorrowRecordConfig());
        mb.ApplyConfiguration(new RefreshTokenConfig());

        // Global soft-delete filter — all queries skip IsDeleted rows
        mb.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        mb.Entity<Author>().HasQueryFilter(a => !a.IsDeleted);
        mb.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);
        mb.Entity<BorrowRecord>().HasQueryFilter(b => !b.IsDeleted);

        SeedData(mb);
    }

    // Intercept SaveChanges to auto-set UpdatedAt
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }

    private static void SeedData(ModelBuilder mb)
    {
        // Seed admin user (password: Admin@123)
        mb.Entity<User>().HasData(new User
        {
            Id           = 1,
            FirstName    = "System",
            LastName     = "Admin",
            Email        = "admin@library.com",
            PasswordHash = "$2a$12$v7tOlMPlMv0gxC8Y/pN6EuY6YhXmCmOeB0B1vYWxP.GkgKkSgGq9y", // Admin@123
            Role         = UserRole.Admin,
            IsActive     = true,
            CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

// ─────────────────────────────────────────────────────────────
// ENTITY TYPE CONFIGURATIONS — Fluent API for clean mapping
// ─────────────────────────────────────────────────────────────
public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);
        b.Property(u => u.Email).HasMaxLength(100).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        b.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        b.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        b.Property(u => u.PhoneNumber).HasMaxLength(20);
        b.Property(u => u.Role).HasConversion<string>();
        b.Ignore(u => u.FullName);   // computed — not mapped
    }
}

public class AuthorConfig : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> b)
    {
        b.HasKey(a => a.Id);
        b.Property(a => a.FirstName).HasMaxLength(50).IsRequired();
        b.Property(a => a.LastName).HasMaxLength(50).IsRequired();
        b.Property(a => a.Nationality).HasMaxLength(60);
        b.Property(a => a.Bio).HasMaxLength(2000);
        b.Ignore(a => a.FullName);
        b.HasMany(a => a.Books)
         .WithOne(bk => bk.Author)
         .HasForeignKey(bk => bk.AuthorId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookConfig : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b)
    {
        b.HasKey(bk => bk.Id);
        b.Property(bk => bk.Title).HasMaxLength(200).IsRequired();
        b.Property(bk => bk.ISBN).HasMaxLength(13).IsRequired();
        b.HasIndex(bk => bk.ISBN).IsUnique();
        b.Property(bk => bk.Description).HasMaxLength(2000);
        b.Property(bk => bk.Fine).HasColumnType("decimal(10,2)");
        b.Property(bk => bk.Genre).HasConversion<string>();
        b.Property(bk => bk.Status).HasConversion<string>();
    }
}

public class BorrowRecordConfig : IEntityTypeConfiguration<BorrowRecord>
{
    public void Configure(EntityTypeBuilder<BorrowRecord> b)
    {
        b.HasKey(br => br.Id);
        b.Property(br => br.FineAmount).HasColumnType("decimal(10,2)");
        b.Property(br => br.Status).HasConversion<string>();
        b.Ignore(br => br.OverdueDays);   // computed
        b.HasOne(br => br.User)
         .WithMany(u => u.BorrowRecords)
         .HasForeignKey(br => br.UserId)
         .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(br => br.Book)
         .WithMany(bk => bk.BorrowRecords)
         .HasForeignKey(br => br.BookId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(rt => rt.Id);
        b.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        b.HasIndex(rt => rt.Token).IsUnique();
        b.HasOne(rt => rt.User)
         .WithMany()
         .HasForeignKey(rt => rt.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
