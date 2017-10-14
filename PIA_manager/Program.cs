using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PIA
{
    // Author Shukhrat Nekbaev
    // Structs and some code snippets were taken from http://www.pinvoke.net/
    class PIA_Runner
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public unsafe byte* lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("kernel32.dll", EntryPoint = "SetEnvironmentVariableA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int SetEnvironmentVariable(string lpName, string lpValue);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(string lpApplicationName,
           string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
           uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
           [In] ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);


        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Multiple arguments, expected 1, exiting...");
                return;
            }
                                                        // default to --run
            var argToPass = args.Length > 0 ? args[0] : "--run";// string.Empty;

            Console.WriteLine("Starting...");

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exeFullPath = Assembly.GetExecutingAssembly().Location;
            var pathToExtractedFiles = Path.Combine(baseDir, "gemhome");

            Console.WriteLine(string.Format("Current directory is: {0}, ", baseDir));
            Console.WriteLine("Setting up environment variables...");

            SetEnvironmentVariable("OCRA_EXECUTABLE", exeFullPath);
            SetEnvironmentVariable("RUBYOPT", null);
            SetEnvironmentVariable("RUBYLIB", null);
            SetEnvironmentVariable("GEM_PATH", pathToExtractedFiles);

            const uint NORMAL_PRIORITY_CLASS = 0x0020;

            bool retValue;
            string application = Path.Combine(baseDir, @"bin\rubyw.exe");

            var tmpCmdLine = Path.Combine(baseDir, @"src\pia_manager.rb");
            string commandLine = string.Format(@"rubyw.exe ""{0}"" {1}", tmpCmdLine, argToPass);

            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();
            STARTUPINFO sInfo = new STARTUPINFO();
            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            //Run
            Console.WriteLine("Executing the application...");
            retValue = CreateProcess(application, commandLine, ref pSec, ref tSec, false, NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref sInfo, out pInfo);

            if (retValue)
            {
                Console.WriteLine("Success...");
                Console.WriteLine("Process ID (PID): " + pInfo.dwProcessId);
                Console.WriteLine("Process Handle : " + pInfo.hProcess);
            }
            else
            {
                Console.WriteLine("Failed...");
            }
        }
    }
}