using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;

namespace MangaManagement.DataAccess.DbContexts;

public class MangaDbContext : DbContext
{
    private const string NewGuidSql = "gen_random_uuid()";

    public MangaDbContext(DbContextOptions<MangaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<UserAssignment> UserAssignments => Set<UserAssignment>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<SeriesGenre> SeriesGenres => Set<SeriesGenre>();
    public DbSet<ProposalPage> ProposalPages => Set<ProposalPage>();
    public DbSet<BoardDecision> BoardDecisions => Set<BoardDecision>();
    public DbSet<BoardVote> BoardVotes => Set<BoardVote>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ChapterPage> ChapterPages => Set<ChapterPage>();
    public DbSet<Manuscript> Manuscripts => Set<Manuscript>();
    public DbSet<PageTask> PageTasks => Set<PageTask>();
    public DbSet<PageTaskSubmission> PageTaskSubmissions => Set<PageTaskSubmission>();
    public DbSet<Annotation> Annotations => Set<Annotation>();
    public DbSet<VoteRecord> VoteRecords => Set<VoteRecord>();
    public DbSet<RankingSnapshot> RankingSnapshots => Set<RankingSnapshot>();
    public DbSet<Escalation> Escalations => Set<Escalation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRoles(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureUserNotifications(modelBuilder);
        ConfigureUserAssignments(modelBuilder);
        ConfigureFileAssets(modelBuilder);
        ConfigureGenres(modelBuilder);
        ConfigureSeries(modelBuilder);
        ConfigureSeriesGenres(modelBuilder);
        ConfigureProposalPages(modelBuilder);
        ConfigureBoardDecisions(modelBuilder);
        ConfigureBoardVotes(modelBuilder);
        ConfigureChapters(modelBuilder);
        ConfigureChapterPages(modelBuilder);
        ConfigureManuscripts(modelBuilder);
        ConfigurePageTasks(modelBuilder);
        ConfigurePageTaskSubmissions(modelBuilder);
        ConfigureAnnotations(modelBuilder);
        ConfigureVoteRecords(modelBuilder);
        ConfigureRankingSnapshots(modelBuilder);
        ConfigureEscalations(modelBuilder);
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.RoleId);

            entity.Property(x => x.RoleId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => x.RoleName).IsUnique();
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.UserName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(150);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(512);
            entity.Property(x => x.RefreshTokenExpiresAt).HasColumnType("timestamptz");
            entity.Property(x => x.LastLoginAt).HasColumnType("timestamptz");

            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(x => x.NotificationId);

            entity.Property(x => x.NotificationId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Link).HasMaxLength(500);
            entity.Property(x => x.Priority).IsRequired().HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");
        });
    }

    private static void ConfigureUserNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(x => x.UserNotificationId);

            entity.Property(x => x.UserNotificationId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.ReadAt).HasColumnType("timestamptz");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.UserId, x.NotificationId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureUserAssignments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAssignment>(entity =>
        {
            entity.ToTable("UserAssignments");
            entity.HasKey(x => x.AssignmentId);

            entity.Property(x => x.AssignmentId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.AssignedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.UnassignedAt).HasColumnType("timestamptz");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_UserAssignments_NotSelf", "\"FromUserId\" <> \"ToUserId\"");
            });

            entity.HasIndex(x => new { x.FromUserId, x.ToUserId })
                .IsUnique()
                .HasFilter("\"UnassignedAt\" IS NULL AND \"DeletedAt\" IS NULL");

            entity.HasOne(x => x.FromUser)
                .WithMany(x => x.AssignmentsFromUser)
                .HasForeignKey(x => x.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToUser)
                .WithMany(x => x.AssignmentsToUser)
                .HasForeignKey(x => x.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFileAssets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileAsset>(entity =>
        {
            entity.ToTable("FileAssets");
            entity.HasKey(x => x.FileAssetId);

            entity.Property(x => x.FileAssetId).HasDefaultValueSql(NewGuidSql);
            //entity.Property(x => x.StorageProvider).IsRequired().HasMaxLength(50);
            entity.Property(x => x.BucketName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.ObjectPath).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(x => x.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(x => x.Extension).IsRequired().HasMaxLength(20);
            entity.Property(x => x.FileSizeBytes).IsRequired();
            //entity.Property(x => x.FileType).IsRequired().HasMaxLength(50);
            entity.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");
        });
    }

    private static void ConfigureGenres(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genres");
            entity.HasKey(x => x.GenreId);

            entity.Property(x => x.GenreId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(100);
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => x.Title).IsUnique();
        });
    }

    private static void ConfigureSeries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Series>(entity =>
        {
            entity.ToTable("Series");
            entity.HasKey(x => x.SeriesId);

            entity.Property(x => x.SeriesId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.PublicationType).IsRequired().HasMaxLength(50);
            entity.Property(x => x.RankingScore).IsRequired().HasPrecision(18, 2).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.Synopsis).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.RejectReason).HasMaxLength(1000);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamptz");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => x.SourceZipFileAssetId)
                .IsUnique()
                .HasFilter("\"SourceZipFileAssetId\" IS NOT NULL");

            entity.HasOne(x => x.Mangaka)
                .WithMany(x => x.Series)
                .HasForeignKey(x => x.MangakaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceZipFileAsset)
                .WithOne(x => x.SeriesSourceZip)
                .HasForeignKey<Series>(x => x.SourceZipFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSeriesGenres(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeriesGenre>(entity =>
        {
            entity.ToTable("SeriesGenres");
            entity.HasKey(x => x.SeriesGenreId);

            entity.Property(x => x.SeriesGenreId).HasDefaultValueSql(NewGuidSql);

            entity.HasIndex(x => new { x.SeriesId, x.GenreId }).IsUnique();

            entity.HasOne(x => x.Series)
                .WithMany(x => x.SeriesGenres)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Genre)
                .WithMany(x => x.SeriesGenres)
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProposalPages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProposalPage>(entity =>
        {
            entity.ToTable("ProposalPages");
            entity.HasKey(x => x.ProposalPageId);

            entity.Property(x => x.ProposalPageId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => x.PreviewFileAssetId).IsUnique();
            entity.HasIndex(x => new { x.SeriesId, x.PageNo }).IsUnique();

            entity.HasOne(x => x.Series)
                .WithMany(x => x.ProposalPages)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviewFileAsset)
                .WithOne(x => x.ProposalPagePreview)
                .HasForeignKey<ProposalPage>(x => x.PreviewFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBoardDecisions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardDecision>(entity =>
        {
            entity.ToTable("BoardDecisions");
            entity.HasKey(x => x.BoardDecisionId);

            entity.Property(x => x.BoardDecisionId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.DecisionType).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Result).HasMaxLength(50);
            entity.Property(x => x.VotingDeadline).IsRequired().HasColumnType("timestamptz");
            entity.Property(x => x.CreatedBy);
            entity.Property(x => x.ExtensionCount).IsRequired().HasDefaultValue(0);
            entity.Property(x => x.ExtendedAt).HasColumnType("timestamptz");
            entity.Property(x => x.ExtensionReason).HasMaxLength(1000);
            entity.Property(x => x.SpecialDecisionAt).HasColumnType("timestamptz");
            entity.Property(x => x.SpecialDecisionReason).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Series)
                .WithMany(x => x.BoardDecisions)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Creator)
                .WithMany(x => x.CreatedBoardDecisions)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SeriesId);

            entity.HasIndex(x => new { x.SeriesId, x.DecisionType, x.Status })
                .IsUnique()
                .HasDatabaseName("IX_BoardDecisions_OpenSeriesProposal_SeriesId")
                .HasFilter("\"DecisionType\" = 'SeriesProposal' AND \"Status\" = 'Open' AND \"DeletedAt\" IS NULL");
        });
    }

    private static void ConfigureBoardVotes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardVote>(entity =>
        {
            entity.ToTable("BoardVotes");
            entity.HasKey(x => x.BoardVoteId);

            entity.Property(x => x.BoardVoteId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.VoteValue).IsRequired();
            entity.Property(x => x.VotedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.Comment).HasMaxLength(1000);
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.BoardDecisionId, x.VoterId }).IsUnique();

            entity.HasOne(x => x.BoardDecision)
                .WithMany(x => x.BoardVotes)
                .HasForeignKey(x => x.BoardDecisionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Voter)
                .WithMany(x => x.BoardVotes)
                .HasForeignKey(x => x.VoterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChapters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.ToTable("Chapters");
            entity.HasKey(x => x.ChapterId);

            entity.Property(x => x.ChapterId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.TotalPages).IsRequired();
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.SeriesId, x.ChapterNo }).IsUnique();

            entity.HasOne(x => x.Series)
                .WithMany(x => x.Chapters)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChapterPages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChapterPage>(entity =>
        {
            entity.ToTable("ChapterPages");
            entity.HasKey(x => x.ChapterPageId);

            entity.Property(x => x.ChapterPageId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.PageNo).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.ChapterId, x.PageNo }).IsUnique();

            entity.HasOne(x => x.Chapter)
                .WithMany()
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ImageFileAsset)
                .WithMany()
                .HasForeignKey(x => x.ImageFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureManuscripts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manuscript>(entity =>
        {
            entity.ToTable("Manuscripts");
            entity.HasKey(x => x.ManuscriptId);

            entity.Property(x => x.ManuscriptId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.FileUrl).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Feedback).HasMaxLength(1000);
            entity.Property(x => x.RevisionCount).IsRequired().HasDefaultValue(0);
            entity.Property(x => x.SubmittedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.ChapterId, x.VersionNo }).IsUnique();

            entity.HasOne(x => x.Chapter)
                .WithMany(x => x.Manuscripts)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Reviewer)
                .WithMany(x => x.ReviewedManuscripts)
                .HasForeignKey(x => x.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePageTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageTask>(entity =>
        {
            entity.ToTable("PageTasks");
            entity.HasKey(x => x.PageTaskId);

            entity.Property(x => x.PageTaskId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.TaskType).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PageTasks_PageRange", "\"PageStart\" <= \"PageEnd\"");
            });

            entity.HasOne(x => x.Chapter)
                .WithMany(x => x.PageTasks)
                .HasForeignKey(x => x.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Assistant)
                .WithMany(x => x.AssistantPageTasks)
                .HasForeignKey(x => x.AssistantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Manuscript)
                .WithMany(x => x.PageTasks)
                .HasForeignKey(x => x.ManuscriptId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePageTaskSubmissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageTaskSubmission>(entity =>
        {
            entity.ToTable("PageTaskSubmissions");
            entity.HasKey(x => x.SubmissionId);

            entity.Property(x => x.SubmissionId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.RejectReason).HasMaxLength(1000);
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasIndex(x => new { x.PageTaskId, x.VersionNo }).IsUnique();

            entity.HasOne(x => x.PageTask)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.PageTaskId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedFileAsset)
                .WithMany(x => x.PageTaskSubmissions)
                .HasForeignKey(x => x.SubmittedFileAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAnnotations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Annotation>(entity =>
        {
            entity.ToTable("Annotations");
            entity.HasKey(x => x.AnnotationId);

            entity.Property(x => x.AnnotationId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.PositionX).HasPrecision(18, 4);
            entity.Property(x => x.PositionY).HasPrecision(18, 4);
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Manuscript)
                .WithMany(x => x.Annotations)
                .HasForeignKey(x => x.ManuscriptId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Author)
                .WithMany(x => x.Annotations)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVoteRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoteRecord>(entity =>
        {
            entity.ToTable("VoteRecords");
            entity.HasKey(x => x.VoteRecordId);

            entity.Property(x => x.VoteRecordId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Period).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_VoteRecords_Count", "\"VoteCount\" <= \"ReaderCount\"");
            });

            entity.HasOne(x => x.Series)
                .WithMany(x => x.VoteRecords)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Confirmer)
                .WithMany(x => x.ConfirmedVoteRecords)
                .HasForeignKey(x => x.ConfirmedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRankingSnapshots(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RankingSnapshot>(entity =>
        {
            entity.ToTable("RankingSnapshots");
            entity.HasKey(x => x.RankingSnapshotId);

            entity.Property(x => x.RankingSnapshotId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Period).IsRequired().HasMaxLength(50);
            entity.Property(x => x.IsBottom20Percent).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Series)
                .WithMany(x => x.RankingSnapshots)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEscalations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Escalation>(entity =>
        {
            entity.ToTable("Escalations");
            entity.HasKey(x => x.EscalationId);

            entity.Property(x => x.EscalationId).HasDefaultValueSql(NewGuidSql);
            entity.Property(x => x.Type).IsRequired().HasMaxLength(50);
            entity.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Priority).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Resolution).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            entity.Property(x => x.DeletedAt).HasColumnType("timestamptz");

            entity.HasOne(x => x.Series)
                .WithMany(x => x.Escalations)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Creator)
                .WithMany(x => x.CreatedEscalations)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Resolver)
                .WithMany(x => x.ResolvedEscalations)
                .HasForeignKey(x => x.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
