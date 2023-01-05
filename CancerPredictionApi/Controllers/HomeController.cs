using CancerPredictionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System;
using System.IO;
using System.Text;

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
        public async Task<IActionResult> UploadVid(IFormFile file)
        {
            var filePath = "D:/down/TEMP.mp4";
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            //Console.WriteLine("starttime:", temp.starttime);
            //Console.WriteLine("endtime:",temp.endtime);
            ServiceResponse<string> response = new ServiceResponse<string>();
            response.Success = true;
            response.Message = "success";
            response.Data = "Temp";
            return Ok(response);

        }
        [HttpPost("/Strip")]
        public async Task<IActionResult> Strip(strip st)
        {
            Console.WriteLine("start time", st.starttime);
            Console.WriteLine("end time", st.endtime);
            return Ok();
        }
        [HttpGet("/download")]
        public async Task<IActionResult> download()
        {
            var filePath = "D:/down/TEMP.mp4";
            //var provider = new FileExtensionContentTypeProvider();
            //if (!provider.TryGetContentType(filePath, out var contentType))
            //{
            //    contentType = "application/octet-stream";
            //}

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "video/mp4", Path.GetFileName(filePath));
        }
        [HttpGet("/dummy")]
        public async Task<IActionResult> dummy()
        {
            string str = detect_cancer(12, 15, 25, 55, @"C:\Users\Gideon\Downloads\TEMP.mp4");
            return Ok(str);
            //return Ok();
        }
        public string detect_cancer(int rsme, int iniw, int finalw, int gain, string loc)
        {
            string lastline;
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"D:\conda\python.exe";
            //start.Arguments = @"D:\App\App.py " + loc + " " + rsme + " " + iniw + " " + finalw + " " + gain;
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
