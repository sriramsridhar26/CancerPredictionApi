using CancerPredictionApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using Python.Runtime;

namespace CancerPredictionApi.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {

        private string _rawFileAddress = "rawAddress";
        private string _downloadFolder = "downloadLoc";
        private string _ffmpegLocation = "ffmpegLoc";
        private string _condaLoc = "condaLoc";
        private string _pythonExeLoc = "pythonExeLoc";
        private string _pythonLogsLoc = "pythonLogsLoc";
        private readonly ILogger<HomeController> _logger;

        //public const string RhdColorTransf = @"D:\App\x64\Debug\ReinhardColorTransfer.dll";

        //[DllImport(RhdColorTransf, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr status(string str);

        //[DllImport(RhdColorTransf, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int transfercolor(string targetname, string sourcename, string outname);
        public HomeController(ILogger<HomeController> logger)
        {
            GetSetting(ref _rawFileAddress);
            GetSetting(ref _downloadFolder);
            GetSetting(ref _ffmpegLocation);
            GetSetting(ref _condaLoc);
            GetSetting(ref _pythonExeLoc);
            GetSetting(ref _pythonLogsLoc);
            _logger = logger;
        }

        [HttpGet("/status")]
        public async Task<IActionResult> status()
        {
            return Ok("Henlo");
        }


        [HttpPost("/UploadVid")]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> UploadVid(IFormFile data, int starttime, int endtime)
        {
            string currentdt = DateTime.Now.ToString("ddMMyyyyhhmmss");
            var filePath = _rawFileAddress + currentdt + ".mp4";
            using (var stream = System.IO.File.Create(filePath))
            {
                await data.CopyToAsync(stream);
            }
            string outname= stripvid(filePath,
                                     TimeSpan.FromSeconds(starttime).ToString(@"hh\:mm\:ss"),
                                     TimeSpan.FromSeconds(endtime).ToString(@"hh\:mm\:ss"),
                                     currentdt);
            ServiceResponse<string> response = new ServiceResponse<string>();
            response.Success = true;
            response.Message = "success";
            response.Data = outname;
            return Ok(response);

        }

        [HttpGet("/stream")]
        public async Task<IActionResult> stream([FromQuery]string filename)
        {

            //string filePath = "D:/down/" + filename+".mp4";
            string filePath = _downloadFolder + filename + ".mp4";

            if(filename == null)
            {
                return BadRequest();
            }
            filePath = "D:/down/" + filename + ".mp4";
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "video/mp4", Path.GetFileName(filePath));
        }

        [HttpPost("/predict")]
        public async Task<IActionResult> predict([FromBody] Param param)
        {
            //string[] locations = new string[2];
            prdreturn response = new prdreturn();
            response.Success = false;
            response.Message = "Issue in backend";
            response.output = null;
            response.workdir = null;
            _logger.LogInformation(param.before, "before");
            _logger.LogInformation(param.now, "now");
            _logger.LogInformation(param.fileName, "filename");
            _logger.LogInformation(param.iniw.ToString(), "iniw");
            _logger.LogInformation(param.gain.ToString(), "gain");

            if (param != null)
            {
                try
                {
                    string outfile=detect_cancer(param);
                    var allText = System.IO.File.ReadAllLines(outfile);
                    var lastLines = allText.Skip(allText.Length - 2);
                    var lastline = lastLines.LastOrDefault();
                    try
                    {
                        response.output = lastLines.LastOrDefault().ToString();
                        response.workdir = lastLines.FirstOrDefault().ToString();
                        //response.Data.output=allText.Skip(allText.Length-1).ToString().Trim();
                        //response.Data.workdir = allText.Skip(allText.Length - 2).ToString().Trim();
                        if(response.output==response.workdir)
                        {
                            response.output = null;
                            response.workdir = null;
                            return BadRequest(response);
                        }
                    }
                    catch(Exception ex)
                    {
                        response.output = ex.ToString();
                        return BadRequest(response);
                    }

                    //if (outpath is null)
                    //{
                    //    return BadRequest(response);
                    //}
                    response.Success = true;
                    response.Message = "success";
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    response.output = ex.Message;
                    return BadRequest(response);
                }
            }
            else
            {
                response.Message = "Please provide all the details!";
                return BadRequest(response);
            }
        }

        //Private methods start here
        private void GetSetting(ref string variable)
        {
            variable = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")[variable];
        }
        private string stripvid(string input, string starttime, string endtime, string currentdt)
        {
            string outname = "Stripped-" + currentdt ;
            string outpath = _downloadFolder + outname + ".mp4";
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.FileName = _ffmpegLocation;
                start.Arguments = "-i " + input + " -ss " + starttime + " -t " + endtime + " -async 1 " + outpath;
                _logger.LogInformation(start.Arguments);
                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                    }
                }
                        return outname;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
           
        }
        //private string preprocess(string vidloc)
        //{

        //    //Importing python
        //    //Runtime.PythonDLL = "D:/conda/python39.dll";
        //    string envPythonHome = "D:/conda/python39.dll";

        //    Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", envPythonHome, EnvironmentVariableTarget.Process);

        //    string pathToVirtualEnv = "D:/conda";
        //    string additional = $"{pathToVirtualEnv};{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib;{pathToVirtualEnv}\\DLLs";
        //    PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + additional;
        //    PythonEngine.Initialize();
        //    PythonEngine.BeginAllowThreads();
        //    PythonEngine.ImportModule()

        //    using (Py.GIL())
        //    {
        //        //var testModule = Py.Import("testModule");
        //        //dynamic dircontrol = Py.Import("dircontrol");
        //        //string workdir = dircontrol.createworkdir();
        //        //return workdir;

        //        //using (var scope = Py.CreateScope())
        //        //{
        //        //    scope.Exec(File.ReadAllText("./dircontrol.py"));
        //        //}
        //        //dynamic np = Py.Import("numpy");
        //        //return (np.cos(np.pi * 2)).ToString();
        //        //st.ToString();


        //        dynamic test = Py.Import("dircontrol");
        //        dynamic r1 = test.createworkdir();
        //        return r1.ToString();
        //    }
        //}

        private string detect_cancer(Param param)
        {
            
            string lastline;
            string vidloc = _downloadFolder + param.fileName + ".mp4";
            string path;
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _condaLoc ;
            if (param.workdir == "null" || param.workdir == null)
            {
                start.Arguments = _pythonExeLoc + param.before + " " + param.now + " " + "null" + " " + vidloc + " " + param.rsme + " " + param.iniw + " " + param.finalw + " " + param.gain;
            }
            else
            {
                start.Arguments = _pythonExeLoc + param.before + " " + param.now + " " + param.workdir + " " + vidloc + " " + param.rsme + " " + param.iniw + " " + param.finalw + " " + param.gain;
            }
            //start.Arguments = _pythonExeLoc + param.before + " "+param.now + " "+ param.workdir+" "+vidloc + " " + param.rsme + " " + param.iniw + " " + param.finalw + " " + param.gain;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            _logger.LogInformation(start.Arguments, "Argument passed");
            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                    path = _pythonLogsLoc + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
                    using (StreamWriter sw = System.IO.File.CreateText(path)) ;
                    System.IO.File.WriteAllText(path, result);
                }
            }

            return path;
        }


        //private string colorconversion(string path)
        //{
        //    string colorpath = path + "/colorconversion";
        //    Directory.CreateDirectory(colorpath);
        //    DirectoryInfo d = new DirectoryInfo(path);
        //    FileInfo[] files = d.GetFiles("*.png");
        //    //Array.Sort(files, 
        //    foreach(FileInfo file in files)
        //    {
        //        transfercolor(path+ "/" + file.Name, "D:/App/1_ml_resize_x6_ml_resize.jpg", colorpath+"/"+file.Name);
        //    }
        //    return colorpath;
        //}
        //private string converttovid(string path)
        //{
        //    ProcessStartInfo start = new ProcessStartInfo();
        //    start.FileName = "D:/ffmpeg/bin/ffmpeg.exe";
        //    start.WindowStyle = ProcessWindowStyle.Hidden;
        //    //start.Arguments = "-i " + filename + " " + dir + "/%04d.jpg";
        //    start.UseShellExecute = false;
        //    start.RedirectStandardOutput = true;
        //    string vidpath = "D:\\App\\super_resolution\\movie.mp4";
        //    start.Arguments = "-f image2 -r 30 -i "+path+"/%d.png -vcodec mpeg4 -y "+vidpath;
        //    using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
        //    {
        //        using (StreamReader reader = process.StandardOutput)
        //        {
        //            string result = reader.ReadToEnd();
        //            Console.WriteLine(result);
        //        }
        //    }
        //    return vidpath;
        //}

       

    }
}
