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
    #region Enumerations
    /// <summary>
    /// Level of the messages to log
    /// </summary>
 
    public enum Level
    {
        /// <summary>Message for debugging</summary>
        Debug,
        /// <summary>Informative messages</summary>
        Info,
        /// <summary>Warnings</summary>
        Warning,
        /// <summary>Error messages</summary>
        Error,
        /// <summary>Critical error</summary>
        Critical
    }
    #endregion    
    
    internal class Logger
    {
        #region Private fields
        private readonly Level level_;
        #endregion

        public Logger(Level level)
        {
            level_ = level;
        }
    
        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="level">Level of the log</param>
        /// <param name="message">Message to log</param>
        public void Log(Level level, string message)
        {
            if(level < level_)
                return;
            Console.WriteLine("[" + level + "] " + message);
        }

        public void Log(Level level, string format, params object[] arg)
        {
            if(level < level_)
                return; 
            Console.WriteLine("[" + level + "] " + format, arg);
        }
    }
}
