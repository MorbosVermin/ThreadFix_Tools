using Com.WaitWha.ThreadFix.REST;
using Com.WaitWha.ThreadFix.REST.Domain;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix
{
    class Program
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Help(string errorMessage = "")
        {
            if (errorMessage.Length > 0)
                Console.Error.WriteLine(errorMessage);

            string filename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine("Syntax: {0} [/info | /add | /upload <filename>] [/team <id|name> | /app <id|name>] [/url <appUrl>] [/tfurl <threadfix URL>] [/debug|/v] <apikey>", filename);
            Console.WriteLine("Examples:");
            Console.WriteLine(@"C:\> {0} fdsa9f89wjkro3kfs0akfdslafk03i03kfowi              #lists teams and saves APIKEY given", filename);
            Console.WriteLine("C:\\> {0} /team \"\"                                         #list teams", filename);
            Console.WriteLine("C:\\> {0} /app \"\"                                          #list apps", filename);
            Console.WriteLine("C:\\> {0} /add /team \"new team name\"                       #create team", filename);
            Console.WriteLine("C:\\> {0} /add /team 3 /app \"app name\" /url \"http://localhost\"   #create application", filename);
            Console.WriteLine(@"C:\> {0} /upload file.xml /app 3                            #uploads a scan file for an application", filename);
            Environment.Exit(-1);
        }

        static void SetupLogging(Level level, string logPath = "")
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            if (logPath.Length > 0)
            {
                PatternLayout patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                patternLayout.ActivateOptions();

                RollingFileAppender fileAppender = new RollingFileAppender();
                fileAppender.Layout = patternLayout;
                fileAppender.File = logPath;
                fileAppender.AppendToFile = true;
                fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
                fileAppender.PreserveLogFileNameExtension = true;
                fileAppender.MaxSizeRollBackups = 7;
                hierarchy.Root.AddAppender(fileAppender);
            }

            PatternLayout consolePatternLayout = new PatternLayout();
            consolePatternLayout.ConversionPattern = "%-5level %message%newline";
            consolePatternLayout.ActivateOptions();

            ConsoleAppender conAppender = new ConsoleAppender();
            conAppender.Layout = consolePatternLayout;
            conAppender.Target = "Console.Out";
            hierarchy.Root.AddAppender(conAppender);

            hierarchy.Root.Level = level;
            hierarchy.Configured = true;
        }

        static void Main(string[] args)
        {
            bool debug = false;
            string logFile = "";
            Uri baseUri = null;
            string action = "info";
            bool team = true;
            int teamId = 0;
            string teamName = "";
            int appId = 0;
            string appName = "";
            string appUrl = "";
            string apikey = "";
            string pathToFile = "";

            if (args.Length == 0)
                Help();

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("/debug") || args[i].Equals("/v"))
                        debug = !debug;

                    else if (args[i].Equals("/log"))
                    {
                        logFile = args[(i + 1)];
                        i++;
                    }
                    else if (args[i].Equals("/tfurl"))
                    {
                        baseUri = new Uri(args[(i + 1)]);
                        i++;
                    }
                    else if (args[i].Equals("/info"))
                    {
                        action = "info";
                    }
                    else if (args[i].Equals("/add"))
                    {
                        action = "add";
                    }
                    else if(args[i].Equals("/upload"))
                    {
                        action = "upload";
                        pathToFile = args[(i + 1)];
                        i++;
                    }
                    else if (args[i].Equals("/team"))
                    {
                        team = true;
                        try
                        {
                            teamId = Int32.Parse(args[(i + 1)]);
                        }
                        catch (Exception)
                        {
                            teamName = args[(i + 1)];
                        }

                        i++;
                    }
                    else if (args[i].Equals("/app"))
                    {
                        team = false;
                        try
                        {
                            appId = Int32.Parse(args[(i + 1)]);
                        }
                        catch (Exception)
                        {
                            appName = args[(i + 1)];
                        }

                        i++;
                    }
                    else if (args[i].Equals("/url"))
                    {
                        appUrl = args[(i + 1)];
                        i++;
                    }
                    else
                        apikey = args[i];

                }
            }
            catch(IndexOutOfRangeException)
            {
                Help("Error: Missing argument. Please check your input and try again.");
            }

            SetupLogging((debug) ? Level.Debug : Level.Info, logFile);

            if(apikey.Length == 0)
            {
                apikey = Properties.Settings.Default.apikey;
                if (apikey.Length == 0)
                    Help("Error: No APIKEY given or in saved settings.");
                else
                    Log.Debug(String.Format("Successfully retrieved APIKEY from settings: {0}", apikey));

            }

            if(baseUri == null && Properties.Settings.Default.threadfix_url.Length > 0)
            {
                baseUri = new Uri(Properties.Settings.Default.threadfix_url);
                Log.Debug(String.Format("Successfully retrieved ThreadFix URL from settings: {0}", baseUri));
            }

            Log.Debug("Saving configuration.");
            Properties.Settings.Default.apikey = apikey;
            if(baseUri != null)
                Properties.Settings.Default.threadfix_url = baseUri.ToString();

            Properties.Settings.Default.Save();

            ThreadFixService service = (baseUri == null) ? new ThreadFixService(apikey) : new ThreadFixService(baseUri, apikey);

            /**
             * TODO
             *  1. Support the user giving just one application or team ID and then just giving information about this ID.
             *  2. Support the addition of manual findings to an application.
             *  
             */
            if(action.Equals("info"))
            {
                List<Team> teams = service.GetAllTeams().GetAwaiter().GetResult();
                foreach(Team t in teams)
                {
                    if(team)
                    {
                        Log.Info(String.Format("[{0}] {1}", t.Id, t.Name));
                        Log.Debug(String.Format("Total Vulns: {0}, Critical: {1}, High: {2}, Medium: {3}, Low: {4}, Info: {5}",
                                t.TotalVulnCount,
                                t.CriticalVulnCount,
                                t.HighVulnCount,
                                t.MediumVulnCount,
                                t.LowVulnCount,
                                t.InfoVulnCount));
                    }else
                    {
                        foreach (Application app in t.Applications)
                        {
                            Log.Info(String.Format("[{0}] {3} - {1} {2}", app.Id, app.Name, app.Url, t.Name));
                            Log.Debug(String.Format("Total Vulns: {0}, Critical: {1}, High: {2}, Medium: {3}, Low: {4}, Info: {5}",
                                app.TotalVulnCount,
                                app.CriticalVulnCount,
                                app.HighVulnCount,
                                app.MediumVulnCount,
                                app.LowVulnCount,
                                app.InfoVulnCount));
                        }
                    }
                }

            }
            else if(action.Equals("add"))
            {
                if(team)
                {
                    if (teamName.Length == 0)
                        Help("Error: Team name is required.");

                    Log.Debug(String.Format("Adding team: {0}", teamName));
                    teamId = service.CreateTeam(teamName).GetAwaiter().GetResult();
                    Log.Info(String.Format("Successfully added team '{0}': {1}", teamName, teamId));
                }
                else
                {
                    if (appName.Length == 0 || appUrl.Length == 0)
                        Help("Error: Application name and URL are required.");
                    else if (teamId == 0)
                        Help("Error: Team ID is required.");

                    Log.Debug(String.Format("Adding application '{0}' to team {1}", appName, teamId));
                    appId = service.AddApplication(teamId, appName, appUrl).GetAwaiter().GetResult();
                    Log.Info(String.Format("Successfully added application '{0}' to team {1}: {2}", appName, teamId, appId));
                }

            }
            else if(action.Equals("upload"))
            {
                if (appId == 0)
                    Help("Error: Application ID is required.");

                if(service.AddApplicationScan(appId, pathToFile).GetAwaiter().GetResult())
                {
                    Log.Info(String.Format("Successfully uploaded scan for application {0}: {1}", appId, pathToFile));
                }else
                {
                    Log.Error(String.Format("Failed to upload scan for application {0}: {1}", appId, pathToFile));
                }

            }
        }
    }
}
