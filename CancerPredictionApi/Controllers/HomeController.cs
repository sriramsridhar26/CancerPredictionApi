using CancerPredictionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System;
using System.IO;
using System.Text;
using MediaToolkit.Model;
using MediaToolkit;
using MediaToolkit.Options;
using static MediaToolkit.Model.Metadata;
using Microsoft.Extensions.Configuration;

namespace CancerPredictionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {

        private string _rawFileAddress = "rawAddress";
        private string _downloadFolder = "downloadLoc";
        private string _ffmpegLocation = "ffmpegLoc";
        private string _condaLoc = "condaLoc";
        private string _pythonExeLoc = "pythonExeLoc";
        private string _pythonLogsLoc = "pythonLogsLoc";

        public HomeController()
        {
            GetSetting(ref _rawFileAddress);
            GetSetting(ref _downloadFolder);
            GetSetting(ref _ffmpegLocation);
            GetSetting(ref _condaLoc);
            GetSetting(ref _pythonExeLoc);
            GetSetting(ref _pythonLogsLoc);
        }

        [HttpGet("/status")]
        public async Task<IActionResult> status()
        {
            return Ok("active");
        }


        [HttpPost("/UploadVid")]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> UploadVid(IFormFile data, int starttime, int endtime)
        {
            string currentdt = DateTime.Now.ToString("ddMMyyyyhhmmss");
            //var filePath = "D:/down/Raw-"+ currentdt +".mp4";
            var filePath = _rawFileAddress + currentdt + ".mp4";
            using (var stream = System.IO.File.Create(filePath))
            {
                await data.CopyToAsync(stream);
            }
            string outname= stripvid(filePath, starttime, endtime, currentdt);
            ServiceResponse<string> response = new ServiceResponse<string>();
            response.Success = true;
            response.Message = "success";
            response.Data = outname;
            return Ok(response);

        }
        //[HttpPost("/Strip")]
        //public async Task<IActionResult> Strip(strip st)
        //{
        //    Console.WriteLine("start time", st.starttime);
        //    Console.WriteLine("end time", st.endtime);
        //    return Ok();
        //}


        [HttpGet("/stream")]
        public async Task<IActionResult> stream([FromQuery]string filename)
        {

            //string filePath = "D:/down/" + filename+".mp4";
            string filePath = _downloadFolder + filename + ".mp4";

            if(filename == null)
            {
                return BadRequest();
            }
            //if(filename == "output_video")
            //{
            //    filePath = "D:/down/" + filename + ".mp4v";
            //}
            //else
            //{
            //    filePath = "D:/down/" + filename + ".mp4";
            //}
            filePath = "D:/down/" + filename + ".mp4";
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "video/mp4", Path.GetFileName(filePath));
        }

        //[HttpGet("/dummy")]
        //public async Task<IActionResult> dummy()
        //{
        //    string str = detect_cancer(12, 15, 25, 55, @"C:\Users\Gideon\Downloads\TEMP.mp4");
        //    return Ok(str);
        //    //return Ok();
        //}

        [HttpPost("/predict")]
        public async Task<IActionResult> predict([FromBody] Param param)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            response.Success = false;
            response.Message = "Issue in backend";
            response.Data = null;

            if (param != null)
            {
                try
                {
                    string outpath=detect_cancer(param);
                    if (outpath is null)
                    {
                        return BadRequest(response);
                    }
                    response.Success = true;
                    response.Message = "success";
                    response.Data = outpath;
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    response.Data = ex.ToString();
                    return BadRequest(response);
                }
            }
            else
            {
                response.Message = "Please provide all the details!";
                return BadRequest(response);
            }
        }
        private void GetSetting(ref string variable)
        {
            variable = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")[variable];
        }
        private string stripvid(string input, int starttime, int endtime, string currentdt)
        {
            string outname = "Stripped-" + currentdt ;
            string path = _downloadFolder;
                //"D:/down/" ;
            string outpath = path + outname + ".mp4";
            var inputfile =  new MediaFile { Filename = input };
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
        private string detect_cancer(Param param)
        {
            string lastline;
            string location = "D:/down/" + param.fileName + ".mp4";
            ProcessStartInfo start = new ProcessStartInfo();
            //start.FileName = @"D:\conda\python.exe";
            //start.Arguments = @"D:\App\App.py " + location + " " + param.rsme + " " + param.iniw + " " + param.finalw + " " + param.gain;
            start.FileName = _condaLoc ;
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


       

    }
}
