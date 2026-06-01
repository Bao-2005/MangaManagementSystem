using MangaManagementSystem.DataAccess.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;

namespace MangaManagement.DataAccess.DbContexts;

public class MangaDbContext : DbContext
{
    private const string NewSequentialIdSql = "NEWSEQUENTIALID()";

    public MangaDbContext(DbContextOptions<MangaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserAssignment> UserAssignments => Set<UserAssignment>();

    //public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Series> Series => Set<Series>();

    public DbSet<Chapter> Chapters => Set<Chapter>();

    public DbSet<Manuscript> Manuscripts => Set<Manuscript>();

    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    public DbSet<ChapterPage> ChapterPages => Set<ChapterPage>();

    public DbSet<PageTask> PageTasks => Set<PageTask>();

    public DbSet<PageTaskSubmission> PageTaskSubmissions => Set<PageTaskSubmission>();

    public DbSet<Annotation> Annotations => Set<Annotation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRoles(modelBuilder);
        ConfigureUserAssignments(modelBuilder);
        //ConfigureUserRoles(modelBuilder);
        ConfigureSeries(modelBuilder);
        ConfigureChapters(modelBuilder);
        ConfigureFileAssets(modelBuilder);
        ConfigureManuscripts(modelBuilder);
        ConfigureChapterPages(modelBuilder);
        ConfigurePageTasks(modelBuilder);
        ConfigurePageTaskSubmissions(modelBuilder);
        ConfigureAnnotations(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.RefreshTokenHash)
                .HasMaxLength(500);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.HasIndex(x => x.UserName)
                .IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(x => x.RoleId);

            entity.Property(x => x.RoleId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.RoleName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.RoleName)
                .IsUnique();
        });
    }

    private static void ConfigureUserAssignments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAssignment>(entity =>
        {
            entity.ToTable("UserAssignments");

            entity.HasKey(x => x.AssignmentId);

            entity.Property(x => x.AssignmentId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(x => x.AssignedAt)
                .IsRequired();

            entity.Property(x => x.UnassignedAt);

            entity.HasOne(x => x.FromUser)
                .WithMany(x => x.AssignmentsFromUser)
                .HasForeignKey(x => x.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToUser)
                .WithMany(x => x.AssignmentsToUser)
                .HasForeignKey(x => x.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.FromUserId);

            entity.HasIndex(x => x.ToUserId)
                .IsUnique()
                .HasFilter("[Status] = 1");
        });
    }

    //private static void ConfigureUserRoles(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<UserRole>(entity =>
    //    {
    //        entity.ToTable("UserRoles");

    //        entity.HasKey(x => new { x.UserId, x.RoleId });

    //        entity.HasOne(x => x.User)
    //            .WithMany(x => x.UserRoles)
    //            .HasForeignKey(x => x.UserId)
    //            .OnDelete(DeleteBehavior.Restrict);

    //        entity.HasOne(x => x.Role)
    //            .WithMany(x => x.UserRoles)
    //            .HasForeignKey(x => x.RoleId)
    //            .OnDelete(DeleteBehavior.Restrict);
    //    });
    //}

    private static void ConfigureSeries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Series>(entity =>
        {
            entity.ToTable("Series");

            entity.HasKey(x => x.SeriesId);

            entity.Property(x => x.SeriesId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Genre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.PublicationType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }

    private static void ConfigureChapters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.ToTable("Chapters");

            entity.HasKey(x => x.ChapterId);

            entity.Property(x => x.ChapterId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Series)
                .WithMany(x => x.Chapters)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.SeriesId, x.ChapterNo })
                .IsUnique();
        });
    }

    private static void ConfigureFileAssets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileAsset>(entity =>
        {
            entity.ToTable("FileAssets");

            entity.HasKey(x => x.FileAssetId);

            entity.Property(x => x.FileAssetId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.BucketName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ObjectPath)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.MimeType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.FileCategory)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.UploadedAt)
                .IsRequired();

            entity.HasIndex(x => x.ObjectPath)
                .IsUnique();

            entity.HasOne(x => x.Uploader)
                .WithMany(x => x.UploadedFiles)
                .HasForeignKey(x => x.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureManuscripts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manuscript>(entity =>
        {
            entity.ToTable("Manuscripts");

            entity.HasKey(x => x.ManuscriptId);

            entity.Property(x => x.ManuscriptId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Feedback)
                .HasMaxLength(2000);

            entity.HasOne(x => x.Chapter)
                .WithMany(x => x.Manuscripts)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.PreviewFileAsset)
                .WithMany()
                .HasForeignKey(x => x.PreviewFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceFileAsset)
                .WithMany()
                .HasForeignKey(x => x.SourceFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ChapterId, x.VersionNo })
                .IsUnique();
        });
    }

    private static void ConfigureChapterPages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChapterPage>(entity =>
        {
            entity.ToTable("ChapterPages");

            entity.HasKey(x => x.ChapterPageId);

            entity.Property(x => x.ChapterPageId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Chapter)
                .WithMany(x => x.ChapterPages)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Manuscript)
                .WithMany(x => x.ChapterPages)
                .HasForeignKey(x => x.ManuscriptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ImageFileAsset)
                .WithMany()
                .HasForeignKey(x => x.ImageFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ManuscriptId, x.PageNo })
                .IsUnique();
        });
    }

    private static void ConfigurePageTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageTask>(entity =>
        {
            entity.ToTable("PageTasks");

            entity.HasKey(x => x.PageTaskId);

            entity.Property(x => x.PageTaskId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.TaskType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Chapter)
                .WithMany(x => x.PageTasks)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Manuscript)
                .WithMany(x => x.PageTasks)
                .HasForeignKey(x => x.ManuscriptId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Assistant)
                .WithMany(x => x.AssignedPageTasks)
                .HasForeignKey(x => x.AssistantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePageTaskSubmissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageTaskSubmission>(entity =>
        {
            entity.ToTable("PageTaskSubmissions");

            entity.HasKey(x => x.SubmissionId);

            entity.Property(x => x.SubmissionId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Note)
                .HasMaxLength(1000);

            entity.Property(x => x.RejectReason)
                .HasMaxLength(1000);

            entity.HasOne(x => x.PageTask)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.PageTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SubmittedFileAsset)
                .WithMany()
                .HasForeignKey(x => x.SubmittedFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PageTaskId, x.VersionNo })
                .IsUnique();
        });
    }

    private static void ConfigureAnnotations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Annotation>(entity =>
        {
            entity.ToTable("Annotations");

            entity.HasKey(x => x.AnnotationId);

            entity.Property(x => x.AnnotationId)
                .HasDefaultValueSql(NewSequentialIdSql);

            entity.Property(x => x.PositionX)
                .HasPrecision(18, 4);

            entity.Property(x => x.PositionY)
                .HasPrecision(18, 4);

            entity.Property(x => x.Content)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Manuscript)
                .WithMany(x => x.Annotations)
                .HasForeignKey(x => x.ManuscriptId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChapterPage)
                .WithMany(x => x.Annotations)
                .HasForeignKey(x => x.ChapterPageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Author)
                .WithMany(x => x.Annotations)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
