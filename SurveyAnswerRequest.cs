using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FormSystem_API2_.Models.DB
{
    /// <summary>
    /// 用於接收提交答案的模型
    /// </summary>
    public class SurveyAnswerRequest
    {
        [Required]
        [StringLength(6, ErrorMessage = "SurveyID 必須為 6 位字符")]
        public string SurveyID { get; set; } = null!;  // 問卷 ID

        [Required]
        [StringLength(50, ErrorMessage = "SurveyContentUnit 長度不得超過 50")]
        public string SurveyContentUnit { get; set; } = null!;  // 問卷單元

        [Required]
        [StringLength(255, ErrorMessage = "SurveyContentQuestion 長度不得超過 255")]
        public string SurveyContentQuestion { get; set; } = null!;  // 問卷問題內容

        [Required]
        public List<string> Answers { get; set; } = new();  // 答案（多選題支持多個答案）

        [StringLength(6, ErrorMessage = "AnswerID 必須為 6 位字符")]
        public string? AnswerID { get; set; }  // 可選的 AnswerID，通常由後端生成
    }

    /// <summary>
    /// SurveyAnswer 資料庫模型
    /// </summary>
    public class SurveyAnswer
    {
        [Key]
        [StringLength(6)]
        public string AnswerID { get; set; } = null!;  // 唯一 AnswerID

        [StringLength(6)]
        [Required]
        public string SurveyID { get; set; } = null!;  // 問卷ID

        [StringLength(255)]
        [Required]
        public string SurveyContentQuestion { get; set; } = null!;  // 問卷中的問題

        [StringLength(255)]
        public string Answer { get; set; } = null!;  // 答案，逗號分隔多選答案

        [StringLength(50)]
        [Required]
        public string SurveyContentUnit { get; set; } = null!;  // 問卷單元

        public DateTime AnswerWhen { get; set; } = DateTime.UtcNow.AddHours(8);  // 默認提交時間 (GMT+8)
    }
}
