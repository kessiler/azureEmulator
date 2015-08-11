#region

using System;
using System.Security.Permissions;
using Azure.Configuration;

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
        [STAThread]

        public static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;

            Console.Clear();
            InitEnvironment();

            while (Azure.IsLive)
            {
                Console.CursorVisible = true;
                ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
            }
        }

        /// <summary>
        /// Initialize the Azure Environment
        /// </summary>
        [MTAThread]
        public static void InitEnvironment()
        {
            if (Azure.IsLive)
                return;

            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            Azure.Initialize();
        }

        /// <summary>
        /// Mies the handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var ex = (Exception)args.ExceptionObject;
            Logging.LogCriticalException(string.Format("SYSTEM CRITICAL EXCEPTION: {0}", ex));
        }
    }
}