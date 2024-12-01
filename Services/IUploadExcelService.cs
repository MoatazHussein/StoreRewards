using StoreRewards.DTOs;

namespace StoreRewards.Services
{
    public interface IUploadExcelService
    {
        Task<UploadExcelResponse> UploadExcelFileAsync(IFormFile file);
    }
}
