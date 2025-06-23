using System;
using System.ComponentModel.DataAnnotations;

namespace FormSystem_API2_.Models
{
    public class SurveyContentRequest
    {
        [Required]
        [MaxLength(6)]
        public string SurveyId { get; set; } // 問卷ID, 6位數

        [MaxLength(6)] // 移除 [Required]，讓它變成可選字段
        public string? SurveyContentID { get; set; } // 每一項目的ID, 6位數

        [MaxLength(50)]
        public string? SurveyContentUnit { get; set; } // 單元名稱，如"基本資料"

        [MaxLength(255)]
        public string? SurveyContentQuestion { get; set; } // 題目內容（如果適用）

        [Required]
        [MaxLength(50)]
        public string? SurveyContentType { get; set; } // 題目類型，如 'text', 'radio', 'range', 'email', 'label' 等

        [MaxLength(255)]
        public string? SurveyContentOption { get; set; } // 選項內容（如果適用）

        [Required]
        public int Order { get; set; } // 單元順序，用於儲存每一單元的順序

        public int? QuestionOrder { get; set; } // 题目顺序，用于记录题目在单元中的顺序，可为空

        public int? OptionOrder { get; set; } // 选项顺序，用于记录选项在题目中的顺序，可为空

        public int? VersionNumber { get; set; } // 新增: 版本號（後端計算填充）
    }
}
