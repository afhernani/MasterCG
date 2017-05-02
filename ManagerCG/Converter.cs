using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ffmpegUI
{
    public class Converter
    {
        #region Properties
        private string _ffExe ="ffmpeg";
        public string ffExe
        {
            get
            {
                return _ffExe;
            }
            set
            {
                _ffExe = value;
            }
        }

        private string _WorkingPath;
        private int d;

        public string WorkingPath
        {
            get
            {
                return _WorkingPath;
            }
            set
            {
                _WorkingPath = value;
            }
        }

        public int FrameRate { get; set; } = 1;

        #endregion

        #region Constructors
        public Converter()
        {
            Initialize();
        }
        public Converter(string ffmpegExePath)
        {
            _ffExe = ffmpegExePath;
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            WorkingPath = Environment.CurrentDirectory;
            //desde la variable de entorno
            var environmentVariable = Environment.GetEnvironmentVariable("Path");
            if (environmentVariable != null)
            {
                string[] mpeg = environmentVariable.Split(Convert.ToChar(";"));
                foreach (string item in mpeg)
                {
                    string cadena = Path.Combine(item, "ffmpeg.exe");
                    if (File.Exists(cadena))
                        ffExe = cadena;
                }
                if (String.IsNullOrEmpty(ffExe))
                    ffExe = Path.Combine(WorkingPath, "ffmpeg.exe");
            }
            else
            {
                ffExe = ffExe = Path.Combine(WorkingPath, "ffmpeg.exe");
            }
        }

        private string GetWorkingFile()
        {
            //try the stated directory
            if (File.Exists(_ffExe))
            {
                return _ffExe;
            }

            //oops, that didn't work, try the base directory
            if (File.Exists(Path.GetFileName(_ffExe)))
            {
                return Path.GetFileName(_ffExe);
            }

            //well, now we are really unlucky, let's just return null
            return null;
        }
        #endregion

        #region Get the File without creating a file lock
        public static System.Drawing.Image LoadImageFromFile(string fileName)
        {
            System.Drawing.Image theImage = null;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open,
            FileAccess.Read))
            {
                byte[] img;
                img = new byte[fileStream.Length];
                fileStream.Read(img, 0, img.Length);
                fileStream.Close();
                theImage = System.Drawing.Image.FromStream(new MemoryStream(img));
                img = null;
            }
            GC.Collect();
            return theImage;
        }

        public static MemoryStream LoadMemoryStreamFromFile(string fileName)
        {
            MemoryStream ms = null;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open,
            FileAccess.Read))
            {
                byte[] fil;
                fil = new byte[fileStream.Length];
                fileStream.Read(fil, 0, fil.Length);
                fileStream.Close();
                ms = new MemoryStream(fil);
            }
            GC.Collect();
            return ms;
        }
        #endregion

        #region Run the process
        private string RunProcess(string Parameters)
        {
            //create a process info
            string ffexe = $"\"{ffExe}\"";
            ProcessStartInfo oInfo = new ProcessStartInfo(ffexe, Parameters);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;

            //Create the output and streamreader to get the output
            string output = null; StreamReader srOutput = null;

            //try the process
            try
            {
                //run the process
                Process proc = System.Diagnostics.Process.Start(oInfo);

                proc.WaitForExit();

                //get the output
                srOutput = proc.StandardError;

                //now put it in a string
                output = srOutput.ReadToEnd();

                proc.Close();
                //proc.Dispose();
            }
            catch (Exception)
            {
                output = string.Empty;
            }
            finally
            {
                //now, if we succeded, close out the streamreader
                if (srOutput != null)
                {
                    srOutput.Close();
                    srOutput.Dispose();
                }
            }
            return output;
        }
        #endregion

        #region GetVideoInfo
        public VideoFile GetVideoInfo(MemoryStream inputFile, string Filename)
        {
            string Workingpath = Path.GetTempPath();
            string tempfile = Path.Combine(WorkingPath, System.Guid.NewGuid().ToString() + Path.GetExtension(Filename));
            FileStream fs = File.Create(tempfile);
            inputFile.WriteTo(fs);
            fs.Flush();
            fs.Close();
            GC.Collect();

            VideoFile vf = null;
            try
            {
                vf = new VideoFile(Filename);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            GetVideoInfo(vf);

            try
            {
                File.Delete(tempfile);
            }
            catch (Exception)
            {

            }

            return vf;
        }
        public VideoFile GetVideoInfo(string inputPath)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            GetVideoInfo(vf);
            return vf;
        }
        public void GetVideoInfo(VideoFile input)
        {
            string video = $"\"{input.Path}\"";
            //set up the parameters for video info
            string Params = string.Format("-i {0}", video);

            string output = RunProcess(Params);
            input.RawInfo = output;

            //get duration
            Regex re = new Regex("[D|d]uration:.((\\d|:|\\.)*)");
            Match m = re.Match(input.RawInfo);

            if (m.Success)
            {
                string duration = m.Groups[1].Value;
                string[] timepieces = duration.Split(new char[] { ':', '.' });
                if (timepieces.Length == 4)
                {
                    input.Duration = new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
                }
            }

            //get audio bit rate
            re = new Regex("[B|b]itrate:.((\\d|:)*)");
            m = re.Match(input.RawInfo);
            double kb = 0.0;
            if (m.Success)
            {
                Double.TryParse(m.Groups[1].Value, out kb);
            }
            input.BitRate = kb;

            //get the audio format
            re = new Regex("[A|a]udio:.*");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                input.AudioFormat = m.Value;
            }

            //get the video format
            re = new Regex("[V|v]ideo:.*");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                input.VideoFormat = m.Value;
            }

            //get the video format
            re = new Regex("(\\d{2,3})x(\\d{2,3})");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                int width = 0; int height = 0;
                int.TryParse(m.Groups[1].Value, out width);
                int.TryParse(m.Groups[2].Value, out height);
                input.Width = width;
                input.Height = height;
            }
            input.infoGathered = true;
        }
        #endregion

        #region Convert to FLV
        public OutputPackage ConvertToFLV(MemoryStream inputFile, string Filename)
        {
            string tempfile = Path.Combine(this.WorkingPath, System.Guid.NewGuid().ToString() + Path.GetExtension(Filename));
            FileStream fs = File.Create(tempfile);
            inputFile.WriteTo(fs);
            fs.Flush();
            fs.Close();
            GC.Collect();

            VideoFile vf = null;
            try
            {
                vf = new VideoFile(tempfile);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            OutputPackage oo = ConvertToFLV(vf);

            try
            {
                File.Delete(tempfile);
            }
            catch (Exception)
            {

            }

            return oo;
        }
        public OutputPackage ConvertToFLV(string inputPath)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            OutputPackage oo = ConvertToFLV(vf);
            return oo;
        }
        public OutputPackage ConvertToFLV(VideoFile input)
        {
            if (!input.infoGathered)
            {
                GetVideoInfo(input);
            }
            OutputPackage ou = new OutputPackage();

            //set up the parameters for getting a previewimage
            string filename = System.Guid.NewGuid().ToString() + ".jpg";
            int secs;

            //divide the duration in 3 to get a preview image in the middle of the clip
            //instead of a black image from the beginning.
            secs = (int)Math.Round(TimeSpan.FromTicks(input.Duration.Ticks / 3).TotalSeconds, 0);

            string finalpath = Path.Combine(this.WorkingPath, filename);
            string Params = string.Format("-i \"{0}\" \"{1}\" -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo", input.Path, finalpath, secs);
            string output = RunProcess(Params);

            ou.RawOutput = output;

            if (File.Exists(finalpath))
            {
                ou.PreviewImage = LoadImageFromFile(finalpath);
                try
                {
                    File.Delete(finalpath);
                }
                catch (Exception) { }
            }
            else
            { //try running again at frame 1 to get something
                Params = string.Format("-y -i \"{0}\" \"{1}\" -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo", input.Path, finalpath, 1);
                output = RunProcess(Params);

                ou.RawOutput = output;

                if (File.Exists(finalpath))
                {
                    ou.PreviewImage = LoadImageFromFile(finalpath);
                    try
                    {
                        File.Delete(finalpath);
                    }
                    catch (Exception) { }
                }
            }
            //-ar 22050 -f flv (o) -y -ar 22050 -ab 64 -f flv
            /* filename = System.Guid.NewGuid().ToString() + ".flv";
            finalpath = Path.Combine(this.WorkingPath, filename);
            Params = string.Format("-i \"{0}\" -ar 22050 -qscale 4 -s 380x284 -f flv \"{1}\"", input.Path, finalpath);
            output = RunProcess(Params);

            if (File.Exists(finalpath))
            {
                ou.VideoStream = LoadMemoryStreamFromFile(finalpath);
                try
                {
                    File.Delete(finalpath);
                }
                catch (Exception) { }
            } */
            return ou;
        }
        #endregion

        #region lista de Imagenes

        public OutputPackage StrackImages(string inputpath, int num)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputpath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            OutputPackage oo = StrackImages(vf, num);
            return oo;
        }

        public OutputPackage StrackImages(VideoFile input, int num)
        {
            if (!input.infoGathered)
            {
                GetVideoInfo(input);
            }
            OutputPackage ou = new OutputPackage();

            double secs;

            //divide the duration in 3 to get a preview image in the middle of the clip
            //instead of a black image from the beginning.
            secs = Math.Round(TimeSpan.FromTicks(input.Duration.Ticks / (num - 1)).TotalSeconds, 0);

            double res = Math.Round(1 / secs, 4);
            string valor = res.ToString().Replace(",", ".");

            //set up the parameters for getting a previewimage
            //string filesearch = "\"%d thrumb\"";//System.Guid.NewGuid().ToString();
            string filename = "tmp-%d.jpg";
            string pathdir = Path.GetDirectoryName(input.Path);
            string finalpath = Path.Combine(pathdir, filename);
            string Params = $"-y -i \"{input.Path}\" -vf fps={valor} \"{finalpath}\"";
            Debug.WriteLine(Params);
            //string output = RunProcess(Params);

            //ou.RawOutput = output;
            ////
            //create a process info
            string ffexe = $"\"{ffExe}\"";
            ProcessStartInfo oInfo = new ProcessStartInfo(ffexe, Params);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = false;
            oInfo.RedirectStandardError = false;

            //Create the output and streamreader to get the output
            //string output = null; StreamReader srOutput = null;

            //try the process
            try
            {
                //run the process
                Process proc = System.Diagnostics.Process.Start(oInfo);

                proc.WaitForExit();

                //get the output
                //srOutput = proc.StandardError;

                //now put it in a string
                //output = srOutput.ReadToEnd();

                proc.Close();
                //proc.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //output = string.Empty;
            }


            Thread.Sleep(2000);
            ///////
            string[] files = Directory.GetFiles(finalpath, $"tmp-*.jpg");
            //Array.Sort(files, StringComparer.Ordinal);
            Array.Sort(files, CompareDinosByLength);

            foreach (var file in files)
            {
                ou.ListImage.Add(LoadImageFromFile(file));
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            //if (File.Exists(finalpath))
            //{
            //    //ou.PreviewImage = LoadImageFromFile(finalpath);
            //    ou.ListImage.Add(LoadImageFromFile(finalpath));
            //    try
            //    {
            //        File.Delete(finalpath);
            //    }
            //    catch (Exception) { }
            //}
            Debug.WriteLine("fin de proceso de extraccion");
            return ou;
        }

        private static int CompareDinosByLength(string x, string y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = x.Length.CompareTo(y.Length);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return x.CompareTo(y);
                    }
                }
            }
        }

        #endregion
        #region meke gif fps=1/60

        public void MakeGifThread(string inputpath, int num)
        {
            t = Task.Factory.StartNew(() => ThreadMakeGif(inputpath, num));
        }

        public void MakeGifThread(VideoFile input, int num)
        {
            t = Task.Factory.StartNew(() => ThreadMakeGif(input, num));
        }

        private void ThreadMakeGif(string inputpath, int num)
        {
            OutputPackage pack = MakeGif(inputpath, num);
            MadeFilmGif?.Invoke(this, pack);
        }

        private void ThreadMakeGif(VideoFile input, int num)
        {
            OutputPackage pack = MakeGif(input, num);
            MadeFilmGif?.Invoke(this, pack);
        }
        public OutputPackage MakeGif(string inputpath, int num)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputpath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            OutputPackage oo = MakeGif(vf, num);
            return oo;
        }
        /// <summary>
        /// Make a Gif animate with framerate 1.
        /// one image for second.
        /// </summary>
        /// <param name="input">VideoFile data input</param>
        /// <param name="num">one frame for each second</param>
        /// <returns></returns>
        public OutputPackage MakeGif(VideoFile input, int num)
        {
            if (!input.infoGathered)
            {
                GetVideoInfo(input);
            }
            OutputPackage ou = new OutputPackage();

            //double secs;

            //divide the duration in 3 to get a preview image in the middle of the clip
            //instead of a black image from the beginning.
            //secs = Math.Round(TimeSpan.FromTicks(input.Duration.Ticks / (num - 1)).TotalSeconds, 0);

            //double res = Math.Round(1 / secs, 4);
            //string valor = res.ToString().Replace(",", ".");

            //set up the parameters for getting a previewimage
            string filesearch = System.Guid.NewGuid().ToString();
            string filename = $"{filesearch}%03d.png";
            string finalpath = Path.Combine(Path.GetDirectoryName(input.Path), filename);
            string Params = $"-y -i \"{input.Path}\" -vf scale=220:-1,fps=1/{num} \"{finalpath}\"";
            Debug.WriteLine(Params);
            //string output = RunProcess(Params);

            //ou.RawOutput = output;
            ////
            //create a process info
            string ffexe = $"\"{ffExe}\"";
            ProcessStartInfo oInfo = new ProcessStartInfo(ffexe, Params);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = false;
            oInfo.RedirectStandardError = false;

            //Create the output and streamreader to get the output
            //string output = null; StreamReader srOutput = null;

            //try the process
            try
            {
                //run the process
                Process proc = System.Diagnostics.Process.Start(oInfo);

                proc.WaitForExit();

                //get the output
                //srOutput = proc.StandardError;

                //now put it in a string
                //output = srOutput.ReadToEnd();

                //ahora montamos el gif con las imagenes creadas en fichero.
                Params = $"-y -framerate {FrameRate} -i \"{finalpath}\"  \"{input.Path}_thumbs_0000.gif\"";
                oInfo = new ProcessStartInfo(ffexe, Params);
                oInfo.UseShellExecute = false;
                oInfo.CreateNoWindow = true;
                oInfo.RedirectStandardOutput = false;
                oInfo.RedirectStandardError = false;
                proc = System.Diagnostics.Process.Start(oInfo);

                proc.WaitForExit();

                if (File.Exists($"{input.Path}_thumbs_0000.gif"))
                {
                    ou.VideoStream = LoadMemoryStreamFromFile($"{input.Path}_thumbs_0000.gif");
                }

                proc.Close();
                //proc.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //output = string.Empty;
            }


            //Thread.Sleep(2000);
            ///////
            string[] files = Directory.GetFiles(Path.GetDirectoryName(input.Path), $"{filesearch}*.png");
            //
            //Array.Sort(files, CompareDinosByLength);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            //if (File.Exists(finalpath))
            //{
            //    //ou.PreviewImage = LoadImageFromFile(finalpath);
            //    ou.ListImage.Add(LoadImageFromFile(finalpath));
            //    try
            //    {
            //        File.Delete(finalpath);
            //    }
            //    catch (Exception) { }
            //}
            Debug.WriteLine($"create gif {input.Path}_thumbs_0000.gif from {input.Path}");
            return ou;
        }

        #endregion

        #region make film gif

        private Task t;

        public delegate void MakeFilmGifHandler(Object sender, OutputPackage package);

        public event MakeFilmGifHandler MadeFilmGif;

        public void MakeFilmGifThread(string inputpath)
        {
            t = Task.Factory.StartNew(() => ThreadMakeFilmGif(inputpath));
        }

        public void MakeFilmGifThread(VideoFile input)
        {
            t = Task.Factory.StartNew(() => ThreadMakeFilmGif(input));
        }

        private void ThreadMakeFilmGif(string inputpath)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputpath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            OutputPackage oo = MakeFilmGif(vf);
            MadeFilmGif?.Invoke(this, oo);
        }
        private void ThreadMakeFilmGif(VideoFile input)
        {
            OutputPackage oo = MakeFilmGif(input);
            MadeFilmGif?.Invoke(this, oo);
        }
        public OutputPackage MakeFilmGif(string inputpath)
        {
            VideoFile vf = null;
            try
            {
                vf = new VideoFile(inputpath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            OutputPackage oo = MakeFilmGif(vf);
            return oo;
        }
        /// <summary>
        /// Make a Gif animate with framerate 1.
        /// one image for second.
        /// </summary>
        /// <param name="input">VideoFile data input</param>
        /// <param name="num">one frame for each second</param>
        /// <returns></returns>
        public OutputPackage MakeFilmGif(VideoFile input)
        {
            if (!input.infoGathered)
            {
                GetVideoInfo(input);
            }
            OutputPackage ou = new OutputPackage();

            

            //set up the parameters for getting a previewimage
            //string filesearch = System.Guid.NewGuid().ToString();
            string filename = $"{input.Path}_thumbs_0000.gif";
            string finalpath = filename;
            string Params = $"-y -i \"{input.Path}\" -vf scale=220:-1 \"{finalpath}\"";
            Debug.WriteLine(Params);
            //string output = RunProcess(Params);

            //ou.RawOutput = output;
            ////
            //create a process info
            string ffexe = $"\"{ffExe}\"";
            ProcessStartInfo oInfo = new ProcessStartInfo(ffexe, Params);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = false;
            oInfo.RedirectStandardError = false;

            //Create the output and streamreader to get the output
            //string output = null; StreamReader srOutput = null;

            //try the process
            try
            {
                //run the process
                Process proc = System.Diagnostics.Process.Start(oInfo);

                proc.WaitForExit();

                //get the output
                //srOutput = proc.StandardError;

                //now put it in a string
                //output = srOutput.ReadToEnd();

                //ahora montamos el gif con las imagenes creadas en fichero.
                //Params = $"-y -framerate 1 -i \"{finalpath}\"  \"{input.Path}_thumbs_0000.gif\"";
                //oInfo = new ProcessStartInfo(ffexe, Params);
                //oInfo.UseShellExecute = false;
                //oInfo.CreateNoWindow = true;
                //oInfo.RedirectStandardOutput = false;
                //oInfo.RedirectStandardError = false;
                //proc = System.Diagnostics.Process.Start(oInfo);

                //proc.WaitForExit();

                if (File.Exists(finalpath))
                {
                    ou.VideoStream = LoadMemoryStreamFromFile(finalpath);
                }

                proc.Close();
                //proc.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //output = string.Empty;
            }


            //Thread.Sleep(2000);
            ///////
            //string[] files = Directory.GetFiles(Path.GetDirectoryName(input.Path), $"{filesearch}*.png");
            ////
            ////Array.Sort(files, CompareDinosByLength);

            //foreach (var file in files)
            //{
            //    try
            //    {
            //        File.Delete(file);
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //}

            //if (File.Exists(finalpath))
            //{
            //    //ou.PreviewImage = LoadImageFromFile(finalpath);
            //    ou.ListImage.Add(LoadImageFromFile(finalpath));
            //    try
            //    {
            //        File.Delete(finalpath);
            //    }
            //    catch (Exception) { }
            //}
            Debug.WriteLine($"create gif {finalpath} from {input.Path}");
            //MadeFilmGif?.Invoke(this, ou);
            return ou;
        }

        #endregion


    }

    public class VideoFile
    {
        #region Properties
        private string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
            }
        }

        public TimeSpan Duration { get; set; }
        public double BitRate { get; set; }
        public string AudioFormat { get; set; }
        public string VideoFormat { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string RawInfo { get; set; }
        public bool infoGathered { get; set; }
        #endregion

        #region Constructors
        public VideoFile(string path)
        {
            _Path = path;
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            this.infoGathered = false;
            //first make sure we have a value for the video file setting
            if (string.IsNullOrEmpty(_Path))
            {
                throw new Exception("Could not find the location of the video file");
            }

            //Now see if the video file exists
            if (!File.Exists(_Path))
            {
                throw new Exception("The video file " + _Path + " does not exist.");
            }
        }
        #endregion
    }

    public class OutputPackage
    {
        public MemoryStream VideoStream { get; set; }
        public System.Drawing.Image PreviewImage { get; set; }
        public List<System.Drawing.Image> ListImage { get; set; } = new List<System.Drawing.Image>();
        public string RawOutput { get; set; }
        public bool Success { get; set; }
    }
}
