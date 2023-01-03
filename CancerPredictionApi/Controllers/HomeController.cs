using CancerPredictionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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
            response.Success= true;
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
            var filePath= "D:/down/TEMP.mp4";
            //var provider = new FileExtensionContentTypeProvider();
            //if (!provider.TryGetContentType(filePath, out var contentType))
            //{
            //    contentType = "application/octet-stream";
            //}

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "video/mp4", Path.GetFileName(filePath));
        }
    }
}
