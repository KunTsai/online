// SurveyContentDbContext 類別，繼承自 DbContext，負責與資料庫進行互動
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using FormSystem_API2_.Models.DB;

public class SurveyContentDbContext : DbContext
{
    public SurveyContentDbContext(DbContextOptions<SurveyContentDbContext> options) : base(options) { }

    public DbSet<SurveyContent> SurveyContent { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SurveyContent>()
            .HasKey(sc => new { sc.SurveyId, sc.SurveyContentID });

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyId)
            .HasMaxLength(6)
            .IsRequired();

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyContentID)
            .HasMaxLength(6)
            .IsRequired();

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyContentUnit)
            .HasMaxLength(50);

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyContentQuestion)
            .HasMaxLength(255);

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyContentType)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.SurveyContentOption)
            .HasMaxLength(255);

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.Order)
            .IsRequired();

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.QuestionOrder)
            .IsRequired(false);

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.OptionOrder)
            .IsRequired(false);

        modelBuilder.Entity<SurveyContent>()
            .Property(sc => sc.VersionNumber) // 新增: 設定版本號屬性
            .IsRequired(); // 必填
    }

}

public class SurveyContent
{
    [MaxLength(6)]
    public string SurveyId { get; set; }  // 問卷 ID

    [MaxLength(6)]
    public string SurveyContentID { get; set; }  // 內容 ID

    public string? SurveyContentUnit { get; set; }  // 單元名稱（可為 NULL）

    public string? SurveyContentQuestion { get; set; }  // 題目內容（可為 NULL）

    [Required]
    [MaxLength(50)]
    public string SurveyContentType { get; set; }  // 題目類型

    public string? SurveyContentOption { get; set; }  // 選項內容（可為 NULL）

    [Required]
    public int Order { get; set; }  // 單元順序

    public int? QuestionOrder { get; set; }  // 题目顺序（可为空）

    public int? OptionOrder { get; set; }  // 选项顺序（可为空）

    [Required]
    public int VersionNumber { get; set; }  // 新增: 版本號（默認必填）
}
