namespace CancerPredictionApi.Model
{
    public class prdreturn
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string workdir { get; set; }
        public string output { get; set; }
    }
}
