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
            {
                Console.Error.WriteLine(errorMessage);
                Console.Error.WriteLine();
            }

            string filename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine("Syntax: {0} [/server <threadfix URL>] [/debug|/v] [<apikey>] [action] [action options...]", filename);
            Console.WriteLine("<apikey>     Is required for at least the first run. Will be saved to a configuration file afterward.");
            Console.WriteLine("/server      Is required for at least the first run. Will be saved to a configuration file afterward.");
            Console.WriteLine();

            Console.WriteLine("Available Actions: ");
            foreach(string name in Enum.GetNames(typeof(Actions)))
            {
                Console.WriteLine("{0}", name + (name.Equals("info") ? " (default)" : ""));
            }
            Console.WriteLine();

            Console.WriteLine("Examples:");
            Console.WriteLine("{0} /teams                                                              #list teams", filename);
            Console.WriteLine("{0} /apps                                                               #list apps", filename);
            Console.WriteLine("{0} /add /teamName \"new team name\"                                    #create team", filename);
            Console.WriteLine("{0} /add /teamId 3 /appName \"app name\" /appUrl \"http://localhost\"   #create application", filename);
            Console.WriteLine("{0} /upload file.xml /appId 3                                           #uploads a scan file for an application", filename);

            Environment.Exit(-1);
        }

        static void SetupLogging(Level level, string logPath = "")
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            if (logPath == null || logPath.Length > 0)
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

        enum Actions
        {
            info,
            add,
            upload
        }

        class Configuration : Dictionary<string, dynamic>
        {
            
            public bool IsDebugMode
            {
                get { return this.Keys.Contains("debug") || this.Keys.Contains("v"); }
            }

            public string LogFile
            {
                get { return (this.Keys.Contains("log")) ? this["log"] : null; }
            }

            public Uri BaseUri
            {
                get { return (this.Keys.Contains("server")) ? new Uri(this["server"]) : null; }
            }

            public Actions Action
            {
                get
                {
                    if (this.Keys.Contains("add"))
                        return Actions.add;
                    else if (this.Keys.Contains("upload"))
                        return Actions.upload;

                    return Actions.info;
                }
            }

            public string ApiKey { get; private set; }

            public Configuration(string[] args)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/") || args[i].StartsWith("-"))
                    {
                        string key = args[i].Substring(1); //everything but the / or -
                        dynamic value;

                        //check next argument, if one exists, to see if it is another option given. This indicates a boolean value.
                        if ((i + 1) < args.Length && !args[(i + 1)].StartsWith("/") && !args[(i + 1)].StartsWith("-"))
                        {
                            value = args[(i + 1)];
                            i++;
                        }
                        else
                            value = true;
                        
                        this.Add(key, value);
                    }
                    else
                    {
                        ApiKey = args[i];
                    }
                }

                SetupLogging((IsDebugMode) ? Level.Debug : Level.Info, LogFile);

                if (ApiKey == null || ApiKey.Length == 0)
                {
                    //try to get API key from Properties.
                    if (Properties.Settings.Default.apikey.Length > 0)
                    {
                        ApiKey = Properties.Settings.Default.apikey;
                        Log.Debug(String.Format("Loaded API KEY from saved settings: {0}", ApiKey));
                    }
                    else
                        Help("Error: No APIKEY given or in saved settings.");

                }

                if(BaseUri == null)
                {
                    //try to get last URI used from properties.
                    if (Properties.Settings.Default.threadfix_url.Length > 0)
                    {
                        this.Add("server", Properties.Settings.Default.threadfix_url);
                        Log.Debug(String.Format("Loaded ThreadFix URL from saved settings: {0}", BaseUri));
                    }
                    else
                        Help("Error: No server given or saved in settings.");

                }
            }

            public dynamic GetValueWithCheck(string key)
            {
                if (this.Keys.Contains(key))
                    return this[key];

                Help("Error: The option '/" + key + "' is required.");
                return null; //unreachable, I know, I know... :/
            }

            public void Save()
            {
                Log.Debug("Saving settings...");
                Properties.Settings.Default.threadfix_url = BaseUri.ToString();
                Properties.Settings.Default.apikey = ApiKey;
                Properties.Settings.Default.Save();
            }
        }
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Help();

            Configuration config = new Configuration(args);
            if (config.Keys.Contains("help") || config.Keys.Contains("h") || config.Keys.Contains("?"))
                Help();
            
            config.Save();

            ThreadFixService service = new ThreadFixService(config.BaseUri, config.ApiKey);
            if(config.Action == Actions.info)
            {
                List<Team> teams = service.GetAllTeams().GetAwaiter().GetResult();
                foreach(Team t in teams)
                {
                    if(config.Keys.Contains("teams"))
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
            else if(config.Action == Actions.add)
            {
                if(config.Keys.Contains("teamName"))
                {
                    string teamName = config.GetValueWithCheck("teamName");

                    Log.Debug(String.Format("Adding team: {0}", teamName));
                    int teamId = service.CreateTeam(teamName).GetAwaiter().GetResult();
                    Log.Info(String.Format("Successfully added team '{0}': {1}", teamName, teamId));

                }
                else if(config.Keys.Contains("teamId"))
                {
                    string appName = config.GetValueWithCheck("appName");
                    string appUrl = config.GetValueWithCheck("appUrl");
                    int teamId = Int32.Parse(config.GetValueWithCheck("teamId"));
                    
                    Log.Debug(String.Format("Adding application '{0}' to team {1}", appName, teamId));
                    int appId = service.AddApplication(teamId, appName, appUrl).GetAwaiter().GetResult();
                    Log.Info(String.Format("Successfully added application '{0}' to team {1}: {2}", appName, teamId, appId));
                }
                else if(config.Keys.Contains("appId"))
                {
                    int appId = Int32.Parse(config.GetValueWithCheck("appId"));
                    string vulnType = config.GetValueWithCheck("vuln");
                    string description = config.GetValueWithCheck("desc");
                    int severity = Int32.Parse(config.GetValueWithCheck("sev"));
                    bool isStatic = (config.Keys.Contains("static")) ? true : false;
                    int nativeId = Int32.Parse(config.GetValueWithCheck("nativeId"));
                    string parameter = (config.Keys.Contains("parameter")) ? config.GetValueWithCheck("parameter") : "";
                    int column = (config.Keys.Contains("column")) ? Int32.Parse(config.GetValueWithCheck("column")) : -1;
                    string filePath = (config.Keys.Contains("filePath")) ? config.GetValueWithCheck("filePath") : "";
                    string lineText = (config.Keys.Contains("lineText")) ? config.GetValueWithCheck("lineText") : "";
                    int lineNumber = (config.Keys.Contains("lineNumber")) ? Int32.Parse(config.GetValueWithCheck("lineNumber")) : 0;
                    string fullUrl = (config.Keys.Contains("fullUrl")) ? config.GetValueWithCheck("fullUrl") : "";
                    string path = (config.Keys.Contains("path")) ? config.GetValueWithCheck("path") : "";

                    Log.Debug(String.Format("Adding manual scan finding for application: {0}", appId));
                    if(service.AddManualFinding(appId, vulnType, description, severity, isStatic, nativeId, parameter, filePath, column, lineText, lineNumber, fullUrl, path).GetAwaiter().GetResult())
                    {
                        Log.Info(String.Format("Successfully added manual finding for application {0}", appId));
                    }
                    else
                    {
                        Log.Error(String.Format("Failed to add manual scan finding for application {0}", appId));
                    }
                }


            }
            else if(config.Action == Actions.upload)
            {
                int appId = Int32.Parse(config.GetValueWithCheck("appId"));
                string pathToFile = config.GetValueWithCheck("upload");

                Log.Info(String.Format("Uploading {0} for application {1}...", pathToFile, appId));
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
