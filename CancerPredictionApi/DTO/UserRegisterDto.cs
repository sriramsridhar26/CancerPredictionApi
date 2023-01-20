namespace CancerPredictionApi.DTO
{
    public class UserRegisterDto
    {
        public string emailId { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string customerName { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
    }
}
