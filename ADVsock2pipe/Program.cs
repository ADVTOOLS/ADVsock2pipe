/*
 * This file is part of ADVsock2pipe
 * Copyright (c) 2011 - ADVTOOLS SARL
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advtools.Advsock2pipe
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point of the console application
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        static void Main(string[] args)
        {
            ShowInformation();
            if(StartServer(args))
                Pause();
        }

        /// <summary>
        /// Show information about the application
        /// </summary>
        private static void ShowInformation()
        {
            Console.WriteLine("ADVsock2pipe version 1.1");
            Console.WriteLine("Copyright (c) 2011 ADVTOOLS - www.advtools.com");
            Console.WriteLine();
        }

        /// <summary>
        /// Start the server with the command-line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool StartServer(string[] args)
        {
            // Get the configuration from the command-line arguments
            Config config = new Config();
            if(!config.Parse(args))
                return false;

            // Create an instance of the server and start it with the configuration
            Server server = new Server();
            return server.Start(config);
        }

        /// <summary>
        /// Pause the application (the main threat) until the user ask it to stop
        /// </summary>
        private static void Pause()
        {
            for(; ; )
            {
                // Let the user stop this application
                Console.WriteLine();
                Console.WriteLine("Press any key to stop...");
                Console.WriteLine();
                Console.ReadKey(true);
                Console.WriteLine("Stop the application? Press Escape to stop");
                ConsoleKeyInfo info = Console.ReadKey(true); 
                if(info.Key == ConsoleKey.Escape)
                    break;
            }
            Console.WriteLine("stopping...");
        }
    }
}
