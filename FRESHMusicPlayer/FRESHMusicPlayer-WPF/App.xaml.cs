﻿using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FRESHMusicPlayer.Handlers;

namespace FRESHMusicPlayer
{
    public enum Skin
    {
        Light, Dark, Classic
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigurationFile Config;
        public static string DataFolderLocation
        {
            get
            {
                if (Directory.Exists("Data")) return "Data";
                else return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer");
            }
        }

        private Window currentWindow;
        private Player player;
        void App_Startup(object sender, StartupEventArgs e )
        {
            LoggingHandler.Log("Handling configuration...");
            Config = ConfigurationHandler.Read();
            player = new Player { Volume = Config.Volume };
            if (Config.Language != "automatic") System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Config.Language);
            ChangeSkin(Config.Theme);

            LoggingHandler.Log("Handling command line args...");

            if (e.Args.Length > 0)
            {
                var args = e.Args.Where(x => x.Contains('.'));
                if (e.Args.Contains("--tageditor")) currentWindow = new TagEditor(args.ToList(), player);
                else currentWindow = new MainWindow(player, args.ToArray());
            }
            else currentWindow = new MainWindow(player);
            currentWindow.Show();
        }
        public static Skin CurrentSkin { get; set; } = Skin.Dark;
        public void ChangeSkin(Skin newSkin)
        {
            CurrentSkin = newSkin;

            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {

                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logPath = Path.Combine(DataFolderLocation, "Logs");
            string fileName = $"\\{DateTime.Now:M.d.yyyy hh mm tt}.txt";
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            File.WriteAllText(logPath + fileName, 
                $"FRESHMusicPlayer {Assembly.GetEntryAssembly().GetName().Version}\n" +
                $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
                $"{Environment.OSVersion.VersionString}\n" +
                $"{e.Exception}");
            var message = string.Format(FRESHMusicPlayer.Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName);
            if (currentWindow is MainWindow maybeWindow)
            {
                try
                {
                    maybeWindow.NotificationHandler.Add(new Handlers.Notifications.Notification
                    {
                        DisplayAsToast = true,
                        IsImportant = true,
                        Type = Handlers.Notifications.NotificationType.Failure,
                        ContentText = message,
                        ButtonText = FRESHMusicPlayer.Properties.Resources.APPLICATION_OPEN_DEBUG_LOG,
                        OnButtonClicked = () =>
                        {
                            System.Diagnostics.Process.Start(logPath);
                            System.Diagnostics.Process.Start(logPath + fileName);
                            return true;
                        }
                    });
                }
                catch
                {
                    MessageBox.Show(message);
                }
            }
            else
            {
                MessageBox.Show(message);
            }
            LoggingHandler.Log($"There was an unhandled exception:\n{e.Exception}");
            e.Handled = true;
        }
    }
}
