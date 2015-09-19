#region

using System;
using System.Security.Permissions;
using Azure.Configuration;
using System.Runtime.InteropServices;

#endregion

namespace Azure
{
    internal class Program
    {
        /// <summary>
        /// Main Void of Azure.Emulator
        /// </summary>
        /// <param name="args">The arguments.</param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            Program.StartEverything();
            while (Azure.IsLive)
            {
                Console.CursorVisible = true;
                ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
            }
        }

        private static void StartEverything()
        {
            Program.StartConsoleWindow();
            Program.DeleteMenu(Program.GetSystemMenu(Program.GetConsoleWindow(), false), Program.SC_CLOSE, 0);
            Program.InitEnvironment();
        }

        public static void StartConsoleWindow()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.SetWindowSize(Console.LargestWindowWidth > 149 ? 150 : Console.WindowWidth, Console.LargestWindowHeight > 49 ? 50 : Console.WindowHeight);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.WriteLine(@"     " + @"                                            |         |              ");
            Console.WriteLine(@"     " + @",---.,---,.   .,---.,---.    ,---.,-.-..   .|    ,---.|--- ,---.,---.");
            Console.WriteLine(@"     " + @",---| .-' |   ||    |---'    |---'| | ||   ||    ,---||    |   ||    ");
            Console.WriteLine(@"     " + @"`---^'---'`---'`    `---'    `---'` ' '`---'`---'`---^`---'`---'`    ");
            Console.WriteLine();
            Console.WriteLine(@"     " + @"  BUILD " + Azure.Version + "." + Azure.Build + " RELEASE 63B CRYPTO BOTH SIDE");
            Console.WriteLine(@"     " + @"  .NET Framework " + Environment.Version + "     C# 6 Roslyn");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(Console.LargestWindowWidth > 149 ? "---------------------------------------------------------------------------------------------------------------------------------------------------" : "-------------------------------------------------------------------------");
        }

        /// <summary>
        /// Initialize the Azure Environment
        /// </summary>
        public static void InitEnvironment()
        {
            if (Azure.IsLive)
                return;

            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += ExceptionHandler;
            Azure.Initialize();
        }

        /// <summary>
        /// Mies the handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var ex = (Exception)args.ExceptionObject;
            Logging.LogCriticalException(string.Format("SYSTEM CRITICAL EXCEPTION: {0}", ex));
        }

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, uint nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        internal const UInt32 SC_CLOSE = 0xF060;
    }
}