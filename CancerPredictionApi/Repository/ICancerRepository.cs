using CancerPredictionApi.Model;

namespace CancerPredictionApi.Repository
{
    public interface ICancerRepository
    {
        Task<string> saveVid(IFormFile data);
        string stripVid(string filePath, int starttime, int endtime);
        string vidpath();
        string predict(Param param);
 
    }
}
