using System;
using System.Diagnostics;
using System.Management;

namespace PIA_manager
{
    static class Extensions
    {
        //Method found here: https://stackoverflow.com/a/40501117
        public static string GetCommandLine(this Process process)
        {
            string cmdLine = null;
            using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
            {
                //The query returns at most 1 match because the process is looked up by ID (which is unique by definition).
                var matchEnum = searcher.Get().GetEnumerator();
                if (matchEnum.MoveNext()) // Move to the 1st item.
                {
                    cmdLine = matchEnum.Current["CommandLine"]?.ToString();
                }
            }
            //Commented out the below because we don't want to throw if this fails - returning null is fine.
            //if (cmdLine == null)
            //{
            //    // Not having found a command line implies 1 of 2 exceptions, which the
            //    // WMI query masked:
            //    // An "Access denied" exception due to lack of privileges.
            //    // A "Cannot process request because the process (<pid>) has exited."
            //    // exception due to the process having terminated.
            //    // We provoke the same exception again simply by accessing process.MainModule.
            //    var dummy = process.MainModule; // Provoke exception.
            //}
            return cmdLine;
        }

        public static void KillTree(this Process process)
        {
            //Force kill the process by its PID and kill all of its children as well
            var args = $"/F /PID {process.Id} /T";
            var info = new ProcessStartInfo("taskkill", args)
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var killer = Process.Start(info))
            {
                //TODO: add a timeout?
                killer.WaitForExit();
            }
        }

        public static bool Contains(this string s, string value, StringComparison comparisonType)
        {
            return s.IndexOf(value, comparisonType) > -1;
        }
    }
}
