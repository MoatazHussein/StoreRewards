using StoreRewards.DTOs;

namespace StoreRewards.Services
{
    public interface IMailService
    {
        Task<ResponseResult> SendEmailAsync(string to, string subject, string body,List<string>? attachmentPaths);

    }
}
