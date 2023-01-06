using System.ComponentModel.DataAnnotations;

namespace CancerPredictionApi.Model
{
    public class UploadVid
    {
        [Required]
        public int starttime { get; set; }
        [Required]
        public int endtime { get; set; }
        [Required]
        public IFormFile data { get; set; }
    }
}
