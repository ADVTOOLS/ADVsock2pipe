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
using NDesk.Options;

namespace Advtools.Advsock2pipe
{
    /// <summary>
    /// Configuration of this application
    /// </summary>
    public class Config
    {
        /// <summary>TCP port number</summary>
        public int Port { get; private set; }
        /// <summary>Name of the pipe</summary>
        public string Pipe { get; private set; }
        /// <summary>Level of the messages to log</summary>
        public Level LogLevel { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Config()
        {
            // Default log level is informative messages
            LogLevel = Level.Info;
        }

        /// <summary>
        /// Parse the command-line arguments
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Return true if the application can continue, false if it has to stop there</returns>
        public bool Parse(string[] args)
        {
            // Help requested (by default, no)?
            bool help = false;

            // Definition of the command-line arguments and how to set the related configuration data
            var options = new OptionSet()
            {
                { "pipe=", "Name of the pipe",          o => Pipe = o },
                { "port=", "TCP port number",           (int o) => Port = o },
                { "log=", "Level of messages to log",   (Level o) => LogLevel = o },
                { "h|help", "Show this message",        o => help = o != null }
            };

            try
            {
                // Parse the command-line arguments (thanks NDesk!)
                List<string> extra = options.Parse(args);
                // If there are some arguments not parsed, no name of pipe or an invalid port number, show the help
                if(extra.Count > 0 || Pipe == null || Port <= 0 || Port > 65535)
                {
                    ShowHelp(options);
                    return false; // Do not continue the application
                }
            }
            catch(OptionException e)
            {
                // Something wrong with the arguments
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'advsock2pipe --help' for more information.");
                return false;
            }

            // Show the help?
            if(help)
            {
                ShowHelp(options);
                return false;
            }

            // Continue the application
            return true;
        }

        /// <summary>
        /// Display some help about this application
        /// </summary>
        /// <param name="options"></param>
        private void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: advsock2pipe [OPTIONS]");
            Console.WriteLine("Redirect data from a TCP port to a Windows named pipe.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
