using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

using System.Diagnostics;


namespace RDPHook35
{
    class Program
    {
        // The matching delegate for GetSystemMetrics
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate bool GetSystemMetricsDelegate(int uType);

        // Import the method so we can call it
        [DllImport("user32.dll")]
        static extern bool GetSystemMetrics(int uType);

        /// <summary>
        /// Our GetSystemMetrics hook handler
        /// </summary>
        static private bool GetSystemMetricsHook(int uType)
        {
            // We aren't going to call the original at all
            // but we could using: return GetSystemMetrics(uType);
            Console.Write("...intercepted...");
            return false;
        }

        /// <summary>
        /// Plays a beep using the native GetSystemMetrics method
        /// </summary>
        static private void PlayGetSystemMetrics()
        {
            const int SM_REMOTESESSION = 0x1000;
            Console.Write("    GetSystemMetrics(SM_REMOTESESSION) return value: ");
            Console.WriteLine(GetSystemMetrics(SM_REMOTESESSION));
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Calling GetSystemMetrics with no hook.");
            PlayGetSystemMetrics();

            Console.Write("\nPress <enter> to call GetSystemMetrics while hooked by GetSystemMetricsHook:");
            Console.ReadLine();

            Console.WriteLine("\nInstalling local hook for user32!GetSystemMetrics");
            // Create the local hook using our GetSystemMetricsDelegate and GetSystemMetricsHook function
            using (var hook = EasyHook.LocalHook.Create(
                    EasyHook.LocalHook.GetProcAddress("user32.dll", "GetSystemMetrics"),
                    new GetSystemMetricsDelegate(GetSystemMetricsHook),
                    null))
            {
                // Only hook this thread (threadId == 0 == GetCurrentThreadId)
                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                PlayGetSystemMetrics();

                runCmd();

                Console.Write("\nPress <enter> to disable hook for current thread:");
                Console.ReadLine();
                Console.WriteLine("\nDisabling hook for current thread.");
                // Exclude this thread (threadId == 0 == GetCurrentThreadId)
                hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
                PlayGetSystemMetrics();

                Console.Write("\nPress <enter> to uninstall hook and exit.");
                Console.ReadLine();
            } // hook.Dispose() will uninstall the hook for us
        }


        static void runCmd()
        {
            Process.Start(@"C:\Program Files (x86)\ABC ENVIRONMENT.exe");
        }
    }
}
