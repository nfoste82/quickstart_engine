//
// Program.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Diagnostics;

namespace QuickStartSampleGame
{
    /// <summary>
    /// Static class containing the main entry-point for the game.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the sample game.
        /// </summary>
        static void Main(string[] args)
        {
            ConfigureEnvironment();            

            // Note: In a real-world game, this would be a try/catch block with an exception logger.
            using(QuickStartSampleGame game = new QuickStartSampleGame())
            {
                game.Run();
            }
        }

        /// <summary>
        /// Configures the run-time environment for all platforms.
        /// </summary>
        static private void ConfigureEnvironment()
        {
            ConfigureWindowsEnvironment();
            ConfigureXboxEnvironment();
        }

        /// <summary>
        /// Configures the run-time environment for the Windows platform.
        /// </summary>
        [Conditional("WINDOWS")]
        static private void ConfigureWindowsEnvironment()
        {
            // @todo:  We really need to find a better way to get at the executing directory.
#if !XBOX
            System.Environment.CurrentDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("QuickStartSampleGame.exe", "");
#endif //!XBOX
        }

        /// <summary>
        /// Configures the run-time environment for the Xbox platform.
        /// </summary>
        [Conditional("XBOX")]
        static private void ConfigureXboxEnvironment()
        {
        }
    }
}

