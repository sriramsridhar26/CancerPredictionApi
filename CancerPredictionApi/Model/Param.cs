using System.ComponentModel.DataAnnotations;

namespace CancerPredictionApi.Model
{
    public class Param
    {
        public int before { get; set; }
        public int now { get; set; }
        public string? workdir { get; set; } = "null";
        [Required]
        public string fileName { get; set; }
        [Required]
        public int rsme { get; set; }
        public int? iniw { get; set; } = 0;
        public int? finalw { get; set; } = 0;
        public double? gain { get; set; } = 0;

    }
}
