using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormSystem_API2_.Models.DB
{
    public class Survey
    {
        [Key]
        [StringLength(6)]
        public string SurveyID { get; set; }  // 6 位隨機數字作為 SurveyID，類型為 string

        [StringLength(255)]
        public string SurveyName { get; set; } = null!;

        [StringLength(50)]
        public string SurveyMode { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime SurveyLastModified { get; set; }

        [StringLength(10)]
        public string UserID { get; set; }  // 將 UserID 更改為 string 類型

        [ForeignKey("UserID")]
        public virtual Users User { get; set; } = null!;

        // 新增 SurveyContent 的集合，用於一對多的關係
        public virtual ICollection<SurveyContent> SurveyContents { get; set; } = new List<SurveyContent>();
    }
}

namespace FormSystem_API2_.Models
{
    public class SurveyRequest
    {
        public string SurveyName { get; set; } = null!;
        public string SurveyMode { get; set; } = null!;
        public string UserID { get; set; } = null!; // 確認大小寫與資料表一致
    }
}
