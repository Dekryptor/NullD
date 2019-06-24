/*
 * Copyright (C) 2011 - 2018 NullD project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using NullD.Common.Logging;
using NullD.Common.MPQ;
using NullD.Common.Storage;
using NullD.Common.Storage.AccountDataBase.Entities;
using NullD.Common.Versions;
using NullD.Core.GS.Items;
using NullD.Core.LogNet.Accounts;
using NullD.Core.LogNet.Commands;
using NullD.Net;
using NullD.Net.GS;
using NullD.Net.LogNet;
using NullD.Core.LogNet.Achievement;
using NullD.Net.WebServices;
using NHibernate.Linq;
using NHibernate.Util;
using Environment = System.Environment;

namespace NullD
{
    /// <summary>
    /// Contains NullD's startup code.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Used for uptime calculations.
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now; // used for uptime calculations.

        /// <summary>
        /// LogNetServer instance.
        /// </summary>
        public static LogNetServer LogNetServer;

        /// <summary>
        /// GameServer instance.
        /// </summary>
        public static GameServer GameServer;

        /// <summary>
        /// LogNetServer thread.
        /// </summary>
        public static Thread LogNetServerThread;

        /// <summary>
        /// GameServer thread.
        /// </summary>
        public static Thread GameServerThread;

        private static readonly Logger Logger = LogManager.CreateLogger(); // logger instance.

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler; // Watch for any unhandled exceptions.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Use invariant culture - we have to set it explicitly for every thread we create to prevent any mpq-reading problems (mostly because of number formats).

            string Revision = "2"; // Ревизия
            string date = "24/06/2019";

            string title = "NullD - Revision [" + Revision + "] - Date [" + date + "] - By AiDiE";

            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Red;
            PrintBanner(); // print ascii banner.
            Console.ResetColor(); // reset color back to default.
            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintLicense();
            Console.ResetColor();

            InitLoggers(); // init logging facility.

            Logger.Info("NullD(DiIiS v2.0) revision {0}. {1}", Revision, date);
            Logger.Info("Required client version: {0}.{1}.", VersionInfo.Ingame.MajorVersion, VersionInfo.LogNet.RequiredClientVersion);

            // init openssl & wrapper.
            try
            {
                Logger.Info("Found OpenSSL version {0}.", OpenSSL.Core.Version.Library.ToString());
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "OpenSSL init error.");
                Console.ReadLine();
                return;
            }

            // prefill the database.
            Common.Storage.AccountDataBase.SessionProvider.RebuildSchema();
            if (!DBSessions.AccountSession.Query<DBAccount>().Any())
            {
                Logger.Info("Initing new database, creating first owner account (test@,123456)");
                var account = AccountManager.CreateAccount("test@", "123456", "test", Account.UserLevels.Owner);
                var gameAccount = GameAccountManager.CreateGameAccount(account);
                account.DBAccount.DBGameAccounts.Add(gameAccount.DBGameAccount);
                account.SaveToDB();
            }

            // init MPQStorage.
            if (!MPQStorage.Initialized)
            {
                Logger.Fatal("Cannot run servers as MPQStorage failed initialization.");
                Console.ReadLine();
                return;
            }

            // load item database.
            Logger.Info("Loading item database..");
            Logger.Trace("Item database loaded with a total of {0} item definitions.", ItemGenerator.TotalItems);

            // load achievements database.
            Logger.Info("Loading achievements database..");
            Logger.Trace("Achievement file parsed with a total of {0} achievements and {1} criteria in {2} categories.",
                AchievementManager.TotalAchievements, AchievementManager.TotalCriteria, AchievementManager.TotalCategories);


            Logger.Info("Type '!commands' for a list of available commands.");

            StartupServers(); // startup the servers
        }

        /// <summary>
        /// Unhandled exception emitter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (e.IsTerminating)
                Logger.FatalException(ex, "NullD terminating because of unhandled exception.");                
            else
                Logger.ErrorException(ex, "Caught unhandled exception.");

            Console.ReadLine();
        }

        #region server startup managment

        private static void StartupServers()
        {
            if(NetworkingConfig.Instance.EnableIPv6)
                Logger.Info("IPv6 enabled!");

            StartLogNet(); // start LogNet.
            StartGS(); // start game-server.

            if(Net.WebServices.Config.Instance.Enabled) // if webservices are enabled,
                StartWebServices(); // start them.

            while (true) // idle loop & command parser
            {
                var line = Console.ReadLine();
                CommandManager.Parse(line);
            }
        }

        public static void Shutdown()
        {
            if (LogNetServer != null)
            {
                Logger.Warn("Shutting down LogNet-Server..");
                LogNetServer.Shutdown();
            }

            if (GameServer != null)
            {
                Logger.Warn("Shutting down Game-Server..");
                GameServer.Shutdown();
            }

            StopWebServices();
            Environment.Exit(0);
        }

        public static bool StartLogNet()
        {
            if (LogNetServer != null) return false;

            LogNetServer = new LogNetServer();
            LogNetServerThread = new Thread(LogNetServer.Run) {IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture};
            LogNetServerThread.Start();
            return true;
        }

        public static bool StopLogNet()
        {
            if (LogNetServer == null) return false;

            Logger.Warn("Stopping LogNet-Server..");
            LogNetServer.Shutdown();
            LogNetServerThread.Abort();
            LogNetServer = null;
            return true;
        }

        public static bool StartGS()
        {
            if (GameServer != null) return false;

            GameServer = new GameServer();
            GameServerThread = new Thread(GameServer.Run) { IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture };
            GameServerThread.Start();

            return true;
        }

        public static bool StopGS()
        {
            if (GameServer == null) return false;

            Logger.Warn("Stopping Game-Server..");
            GameServer.Shutdown();
            GameServerThread.Abort();
            GameServer = null;

            return true;
        }

        public static bool StartWebServices()
        {
            Environment.SetEnvironmentVariable("MONO_STRICT_MS_COMPLIANT", "yes"); // we need this here to make sure web-services also work under mono too. /raist.
			
            var webservices = new ServiceManager();
            webservices.Run();

            return true;
        }

        public static bool StopWebServices()
        {
            // todo: stopping webservices if started

            return true;
        }

        #endregion

        #region logging facility 

        /// <summary>
        /// Inits logging facility and loggers.
        /// </summary>
        private static void InitLoggers()
        {
            LogManager.Enabled = true; // enable logger by default.

            foreach (var targetConfig in LogConfig.Instance.Targets)
            {
                if (!targetConfig.Enabled)
                    continue;

                LogTarget target = null;
                switch (targetConfig.Target.ToLower())
                {
                    case "console":
                        target = new ConsoleTarget(targetConfig.MinimumLevel, targetConfig.MaximumLevel,
                                                   targetConfig.IncludeTimeStamps);
                        break;
                    case "file":
                        target = new FileTarget(targetConfig.FileName, targetConfig.MinimumLevel,
                                                targetConfig.MaximumLevel, targetConfig.IncludeTimeStamps,
                                                targetConfig.ResetOnStartup);
                        break;
                }

                if (target != null)
                    LogManager.AttachLogTarget(target);
            }
        }

        #endregion

        #region console banners

        /// <summary>
        /// Prints an info banner.
        /// </summary>
        private static void PrintBanner()
        {
            Console.WriteLine("                       This is a special edition of the server for absolutely free expansion.               ");
            Console.WriteLine("                                        https://github.com/iEvengel/NullD.git                               ");
            Console.WriteLine();
        }

        /// <summary>
        /// Prints a copyright banner.
        /// </summary>
        private static void PrintLicense()
        {
            Console.WriteLine("                               Copyright (C) 2019. Created by AiDiEvE. aka (iEvE)                ");
            Console.WriteLine("                    Anyone wishing to help the project, contact the Discord Channel - DiIiS.          ");
            Console.WriteLine(" If you wish, you can provide any financial assistance, Welcome to Patreon- and you will receive the latest updates.      ");
            Console.WriteLine("         Discord - https://discord.gg/FJVjY4k --------------------- Patreon - https://www.patreon.com/DiIiS");
            Console.WriteLine("");
            Console.WriteLine();
        }

        #endregion
    }
}
