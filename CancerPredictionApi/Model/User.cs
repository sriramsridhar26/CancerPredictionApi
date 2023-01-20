using System.ComponentModel.DataAnnotations;

namespace CancerPredictionApi.Model
{
    public class User
    {

        [Key]
        public string emailId { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string customerName { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
    }
}
