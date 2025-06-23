using Microsoft.AspNetCore.Mvc;
using FormSystem_API2_.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FormSystem_API2_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyAnswerController : ControllerBase
    {
        private readonly SurveyAnswerDbContext _context;

        public SurveyAnswerController(SurveyAnswerDbContext context)
        {
            _context = context;
        }

        [HttpPost("SubmitSurveyAnswers")]
        public async Task<IActionResult> SubmitSurveyAnswers([FromBody] List<SurveyAnswerRequest> surveyAnswers)
        {
            if (surveyAnswers == null || surveyAnswers.Count == 0)
            {
                return BadRequest(new { success = false, message = "答案資料不可為空" });
            }

            // 從控制器生成唯一的 AnswerID
            string answerID = await GenerateUniqueAnswerIDAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var answerRequest in surveyAnswers)
                {
                    var surveyAnswer = new SurveyAnswer
                    {
                        AnswerID = answerID, // 使用後端生成的唯一 AnswerID
                        SurveyID = answerRequest.SurveyID,
                        SurveyContentUnit = answerRequest.SurveyContentUnit,
                        SurveyContentQuestion = answerRequest.SurveyContentQuestion,
                        Answer = string.Join(",", answerRequest.Answers), // 將多選題答案合併為逗號分隔
                        AnswerWhen = DateTime.UtcNow.AddHours(8)
                    };

                    _context.SurveyAnswers.Add(surveyAnswer);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "答案已提交", answerID }); // 回傳 AnswerID 作為響應
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"提交失敗: {ex.Message}");

                // 返回統一格式的 JSON 錯誤響應
                return StatusCode(500, new { success = false, message = $"提交過程中出現錯誤: {ex.Message}" });
            }
        }

        // 生成唯一 AnswerID 的方法
        private async Task<string> GenerateUniqueAnswerIDAsync()
        {
            string newId;
            bool isDuplicate;

            do
            {
                newId = new Random().Next(100000, 999999).ToString("D6");
                isDuplicate = await _context.SurveyAnswers.AsNoTracking().AnyAsync(sa => sa.AnswerID == newId);
            }
            while (isDuplicate);

            return newId;
        }

        // 根據 SurveyID 抓取問卷的提交答案
        [HttpGet("GetSurveyAnswers/{surveyId}")]
        public async Task<IActionResult> GetSurveyAnswers(string surveyId)
        {
            var answers = await _context.SurveyAnswers
                                        .Where(sa => sa.SurveyID == surveyId)
                                        .Select(sa => new
                                        {
                                            sa.AnswerID,  // 包含 AnswerID
                                            sa.SurveyContentUnit,
                                            sa.SurveyContentQuestion,
                                            sa.Answer,
                                            sa.AnswerWhen
                                        })
                                        .ToListAsync();

            if (answers == null || !answers.Any())
            {
                return NotFound(new { success = false, message = "沒有找到該問卷的答案資料" });
            }

            return Ok(new { success = true, answers });
        }
    }
}
