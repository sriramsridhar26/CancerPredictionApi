using CancerPredictionApi.Model;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit;
using System.Diagnostics;

namespace CancerPredictionApi.Repository
{
    public class CancerRepository : ICancerRepository
    {
        private string _rawFileAddress = "rawAddress";
        private string _downloadFolder = "downloadLoc";
        private string _ffmpegLocation = "ffmpegLoc";
        private string _condaLoc = "condaLoc";
        private string _pythonExeLoc = "pythonExeLoc";
        private string _pythonLogsLoc = "pythonLogsLoc";

        private string currentdt = DateTime.Now.ToString("ddMMyyyyhhmmss");
        public CancerRepository()
        {
            getSetting(ref _rawFileAddress);
            getSetting(ref _downloadFolder);
            getSetting(ref _ffmpegLocation);
            getSetting(ref _condaLoc);
            getSetting(ref _pythonExeLoc);
            getSetting(ref _pythonLogsLoc);
        }

        private void getSetting(ref string variable)
        {
            variable = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")[variable];
        }
        public async Task<string> saveVid(IFormFile data)
        {
            var filePath = _rawFileAddress + currentdt + ".mp4";
            using (var stream = System.IO.File.Create(filePath))
            {
                await data.CopyToAsync(stream);
            }
            return filePath;
        }
        public string stripVid(string filePath, int starttime, int endtime)
        {
            string outname = "Stripped-" + currentdt;
            string path = _downloadFolder;
            //"D:/down/" ;
            string outpath = path + outname + ".mp4";
            var inputfile = new MediaFile { Filename = filePath };
            var outputfile = new MediaFile { Filename = outpath };
            try
            {
                //using (var engine = new Engine(@"C:\Users\Gideon\source\repos\CancerPredictionApi\CancerPredictionApi\ffmpeg\bin\ffmpeg.exe"))
                using (var engine = new Engine(_ffmpegLocation))
                {
                    engine.GetMetadata(inputfile);
                    var options = new ConversionOptions();

                    // This example will create a 25 second video, starting from the 
                    // 30th second of the original video.
                    // First parameter requests the starting frame to cut the media from.
                    // Second parameter requests how long to cut the video.
                    options.CutMedia(TimeSpan.FromSeconds(starttime), TimeSpan.FromSeconds(endtime - starttime + 1));
                    engine.Convert(inputfile, outputfile, options);
                    return outname;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }

        }

        public string predict(Param param)
        {

       
            string outpath = detectCancer(param);

            return outpath;

        }

        private string detectCancer(Param param)
        {
            string lastline;
            string location = "D:/down/" + param.fileName + ".mp4";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _condaLoc;
            start.Arguments = _pythonExeLoc + location + " " + param.rsme + " " + param.iniw + " " + param.finalw + " " + param.gain;

            //start.Arguments = @"D:\App\App.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                    //string path = @"D:\down\logs\" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
                    string path = _pythonLogsLoc + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
                    using (StreamWriter sw = System.IO.File.CreateText(path)) ;
                    System.IO.File.WriteAllText(path, result);
                    lastline = System.IO.File.ReadLines(path).Last();
                    Console.WriteLine(lastline.Trim());
                }
            }
            return lastline;
        }

        public string vidpath()
        {
            return _downloadFolder;
        }


    }
}
