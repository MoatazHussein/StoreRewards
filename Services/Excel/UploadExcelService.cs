using ExcelDataReader;
using StoreRewards.DTOs;
using System.Data;

namespace StoreRewards.Services.Excel
{
    public class UploadExcelService : IUploadExcelService
    {
        private readonly ILogger<UploadExcelService> _logger;

        public UploadExcelService(ILogger<UploadExcelService> logger)
        {
            _logger = logger;
        }

        public async Task<UploadExcelResponse> UploadExcelFileAsync(IFormFile file)
        {
            var response = new UploadExcelResponse
            {
                Users = new List<UserMail>(),
                Success = true,
                Message = "File processed successfully."
            };

            try
            {
                if (file == null || file.Length == 0)
                {
                    response.Success = false;
                    response.Message = "No file uploaded.";
                    return response; // Return immediately if no file is uploaded
                }

                if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
                {
                    response.Success = false;
                    response.Message = "Invalid file format. Only .xls and .xlsx are supported.";
                    return response; // Return immediately if no file is uploaded
                }

                // Offload the file processing to a background thread since ExcelDataReader is synchronous
                response.Users = await Task.Run(() =>
                {
                    var userList = new List<UserMail>();

                    // Process the Excel file using ExcelDataReader 
                    using (var stream = file.OpenReadStream())
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true // First row is considered header
                            }
                        });

                        var dataTable = dataSet.Tables[0]; // Get the first worksheet

                        // Process data from the table
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var name = row["Name"].ToString();
                            var email = row["Email"].ToString();

                            // Basic validation before adding to the list
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                            {
                                userList.Add(new UserMail
                                {
                                    Name = name,
                                    Email = email
                                });
                            }
                            else
                            {
                                _logger.LogWarning($"Missing Name or Email at row {row.Table.Rows.IndexOf(row) + 1}");
                            }
                        }
                    }

                    return userList;
                });

                // If no valid users were processed, update the response message
                if (!response.Users.Any())
                {
                    response.Success = false;
                    response.Message = "No valid data found in the file.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the Excel file.");
                response.Success = false;
                response.Message = "An error occurred while processing the file.";
            }

            return response;
        }
    }
}