namespace StoreRewards.DTOs
{
    public class UploadExcelResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<UserMail>? Users { get; set; }
    }

}
