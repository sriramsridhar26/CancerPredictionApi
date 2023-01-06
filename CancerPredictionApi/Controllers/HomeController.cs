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

namespace CancerPredictionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {

        [HttpGet("/status")]
        public async Task<IActionResult> status()
        {
            return Ok("active");
        }
        [HttpPost("/UploadVid")]
        public async Task<IActionResult> UploadVid(IFormFile data, int starttime, int endtime)
        {
            string currentdt = DateTime.Now.ToString("ddMMyyyyhhmmss");
            var filePath = "D:/down/Raw-"+ currentdt +".mp4";
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
            string filePath = "D:/down/" + filename+".mp4";
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

        private string stripvid(string input, int starttime, int endtime, string currentdt)
        {
            string outname = "Stripped-" + currentdt ;
            string path = "D:/down/" ;
            string outpath = path + outname + ".mp4";
            var inputfile =  new MediaFile { Filename = input };
            var outputfile = new MediaFile { Filename = outpath };
            try
            {
                using (var engine = new Engine(@"C:\Users\Gideon\source\repos\CancerPredictionApi\CancerPredictionApi\ffmpeg\bin\ffmpeg.exe"))
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
        private string detect_cancer(int rsme, int iniw, int finalw, int gain, string filename)
        {
            string lastline;
            string location = "D:/down/" + filename + ".mp4";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"D:\conda\python.exe";
            //start.Arguments = @"D:\App\App.py " + location + " " + rsme + " " + iniw + " " + finalw + " " + gain;
            start.Arguments = @"D:\App\App.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                    string path = @"D:\" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
                    using (StreamWriter sw = System.IO.File.CreateText(path)) ;
                    System.IO.File.WriteAllText(path, result);
                    lastline = System.IO.File.ReadLines(path).Last();
                    Console.WriteLine(lastline);
                }
            }
            return lastline;
        }

    }
}
