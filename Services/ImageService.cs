
using StoreRewards.DTOs;
using System.IO;

namespace StoreRewards.Services
{
    public class ImageService
    {
        private readonly string _imagesDirectory;
        private const long MaxFileSize = 2 * 1024 * 1024;
        private readonly string[] _validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        public ImageService()
        {
            _imagesDirectory = Path.Combine(Environment.GetEnvironmentVariable("UploadedImagesPath") ?? "C:", Environment.GetEnvironmentVariable("UploadedImagesFolder") ?? "");
        }
        public async Task<ImageSaveResult> ValidateImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return new ImageSaveResult
                {
                    Success = false,
                    ErrorMessage = "No file uploaded."
                };
            }
            // Validate file size
            if (file.Length > MaxFileSize)
            {
                return new ImageSaveResult
                {
                    Success = false,
                    ErrorMessage = "File is too large. Maximum size is 2 MB."
                };
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_validImageExtensions.Contains(fileExtension))
            {
                return new ImageSaveResult
                {
                    Success = false,
                    ErrorMessage = "Invalid file type. Only image files are allowed."

                };
            }
            return new ImageSaveResult
            {
                Success = true,
            };
        }
        public async Task<ImageSaveResult> SaveImageAsync(string destinationFolder, IFormFile file)
        {
            var imageValidationResult = await ValidateImage(file);

            if (!imageValidationResult.Success)
            {
                return new ImageSaveResult
                {
                    Success = false,
                    ErrorMessage = imageValidationResult.ErrorMessage
                };
            }

            var imageFullDirectory = Path.Combine(_imagesDirectory, destinationFolder);
            if (!Directory.Exists(imageFullDirectory))
            {
                Directory.CreateDirectory(imageFullDirectory);
            }

            var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);

            var absoluteFilePath = Path.Combine(imageFullDirectory, fileName);

            var relativeFilePath = Path.Combine(Environment.GetEnvironmentVariable("UploadedImagesFolder") ?? "C:", destinationFolder, fileName);

            try
            {
                // Save the file to the server
                using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return new ImageSaveResult
                {
                    Success = true,
                    FilePath = relativeFilePath
                };
            }
            catch (Exception ex)
            {
                return new ImageSaveResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while saving the file: {ex.Message}"
                };
            }

        }
        public async Task DeleteImageAsync(string filePath)
        {
            var imageFullDirectory = Path.Combine(Environment.GetEnvironmentVariable("UploadedImagesPath"), filePath);

            if (File.Exists(imageFullDirectory))
            {
                File.Delete(imageFullDirectory);
            }
        }

    }
}
