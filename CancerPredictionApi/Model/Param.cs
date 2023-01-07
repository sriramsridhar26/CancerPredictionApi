using System.ComponentModel.DataAnnotations;

namespace CancerPredictionApi.Model
{
    public class Param
    {
        [Required]
        public string fileName { get; set; }
        [Required]
        public int rsme { get; set; }
        [Required]
        public int iniw { get; set; }
        [Required]
        public int finalw { get; set; }
        [Required]
        public int gain { get; set; }

    }
}
