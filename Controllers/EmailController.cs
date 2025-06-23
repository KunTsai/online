using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using FormSystem_API2_.Models.DB;

namespace SendEmail.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IConfiguration _config;
        private readonly UserManagementContext _context;
        private static Dictionary<string, (string Code, DateTime Expiry)> verificationCodes = new Dictionary<string, (string Code, DateTime Expiry)>();
        private static Dictionary<string, DateTime> emailSendTimes = new Dictionary<string, DateTime>();

        public EmailController(ILogger<EmailController> logger, IConfiguration config, UserManagementContext context)
        {
            _logger = logger;
            _config = config;
            _context = context;
        }

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailEntity emailEntity)
        {
            if (string.IsNullOrEmpty(emailEntity.ToEmailAddress))
            {
                return BadRequest("電子郵件地址是必填項");
            }

            if (emailSendTimes.ContainsKey(emailEntity.ToEmailAddress) &&
                (DateTime.Now - emailSendTimes[emailEntity.ToEmailAddress]).TotalSeconds < 60)
            {
                return BadRequest(new { success = false, message = "請稍等片刻再重新發送" });
            }

            emailSendTimes[emailEntity.ToEmailAddress] = DateTime.Now;

            try
            {
                // 检查邮箱是否已注册过
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailEntity.ToEmailAddress);
                if (existingUser != null)
                {
                    return BadRequest(new { success = false, message = "該電子郵件已被註冊" });
                }

                // 生成验证码
                string verificationCode = GenerateVerificationCode();
                string subject = "您的驗證碼";
                string body = $"您的驗證碼是：{verificationCode}";

                // 保存验证码及其过期时间
                verificationCodes[emailEntity.ToEmailAddress] = (verificationCode, DateTime.Now.AddMinutes(5)); // 5分钟有效

                // 读取配置的邮箱发送设置
                var username = _config.GetValue<string>("EmailConfig:Username");
                var password = _config.GetValue<string>("EmailConfig:Password");
                var host = _config.GetValue<string>("EmailConfig:Host");
                var port = _config.GetValue<int>("EmailConfig:Port");
                var fromEmail = _config.GetValue<string>("EmailConfig:FromEmail");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
                {
                    return StatusCode(500, new { success = false, message = "郵件配置缺失" });
                }

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body
                };
                message.To.Add(emailEntity.ToEmailAddress);

                using (SmtpClient mailClient = new SmtpClient(host))
                {
                    mailClient.UseDefaultCredentials = false;
                    mailClient.Credentials = new System.Net.NetworkCredential(username, password);
                    mailClient.Port = port;
                    mailClient.EnableSsl = true;

                    await mailClient.SendMailAsync(message); // 异步发送邮件
                }

                return Ok(new { success = true, message = "驗證碼已發送" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送驗證碼郵件時出錯");
                return StatusCode(500, new { success = false, message = "服務器錯誤，請稍後再試" });
            }
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code.ToString("D6");
        }

        [HttpPost("VerifyCode")]
        public IActionResult VerifyCode([FromBody] VerificationRequest request)
        {
            if (verificationCodes.TryGetValue(request.Email, out var storedCode))
            {
                if (storedCode.Code == request.Code && DateTime.Now <= storedCode.Expiry)
                {
                    return Ok(new { success = true, message = "驗證碼正確" });
                }
                else if (DateTime.Now > storedCode.Expiry)
                {
                    return BadRequest(new { success = false, message = "驗證碼已過期" });
                }
                else
                {
                    return BadRequest("驗證碼不正確" );
                }
            }
            return BadRequest(new { success = false, message = "找不到該電子郵件。" });
        }
    }

    public class EmailEntity
    {
        public string ToEmailAddress { get; set; }
    }

    public class VerificationRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
