using Microsoft.AspNetCore.Mvc;
using StoreRewards.Data;
using StoreRewards.Services;
using Microsoft.AspNetCore.Authorization;

namespace StoreRewards.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmailsController : AppBaseController
    {

        private readonly AuthService _authService;
        private readonly IMailService _mailService;
        private readonly IUploadExcelService _uploadExcelService;

        public EmailsController(DataContext context, IMailService mailService,
            AuthService authService, ILogger<UsersController> logger, IUploadExcelService uploadExcelService) : base(context)
        {
            _mailService = mailService;
            _authService = authService;
            _uploadExcelService = uploadExcelService;
        }

        [HttpPost(nameof(UploadRecipients))]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadRecipients(IFormFile file)
        {
            var response = await _uploadExcelService.UploadExcelFileAsync(file);

            if (!response.Success)
            {
                return BadRequest(new { msg = response.Message });
            }

            var targetUsers = response.Users;
            var errorsList = new List<string>();

            if (targetUsers is not null)
            {
                foreach (var user in targetUsers)
                {
                    var name = user.Name;
                    var mail = user.Email;

                    if (name is not null && mail is not null)
                    {
                        var subject = $"اربح 50,000 ألف ريال مجاناً بدون رأس مال 💲 ";
                        var message = @$" <div style=""direction: rtl; text-align: right; font-family: Arial, sans-serif; line-height: 1.5;"">
                                        يا هلا فيك {name} نورتنا 👋🏻
                                       <br>
                                                واسعدنا طلب انضمامك 
                                       <br>
                                            جبنا لك الفرصة لحد عندك  
                                        <br>
                                        <br>
                       ارفقنا لك ملف (PDF) فيه كامل تفاصيل الفرصة 🔖
<br>
                تقدر تشوفها وتنطلق على بركة الله وفالك المليون يا رب     🤲🏻
<br>
<br>
 <b> منيرة بزنس</b>
<br>
<b> فريق التسويق</b>
</div>
";

                        var attachmentPaths = new List<string> { "C:\\General-Data\\Mail Data\\Prize!!.PDF"};
                        var emailSendResult = await _mailService.SendEmailAsync(mail, subject, message, attachmentPaths);
                        if (!emailSendResult.Success && !string.IsNullOrEmpty(emailSendResult?.ErrorMessage))
                        {
                            errorsList.Add(emailSendResult.ErrorMessage);
                        }
                    }
                }

                if (errorsList.Count == 0)
                {
                    return Ok(new { msg = $"Emails have been sent successfully to the users", users = targetUsers });
                }
                else
                {
                    return StatusCode(207, new { msg = $"Emails have been sent to the users with errors:", err = errorsList });
                }
            }

            else
            {
                return StatusCode(404, new { msg = $"there are no users to mail:" });
            }
        }

    }

}
