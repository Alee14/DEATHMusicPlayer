﻿using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public ConfigurationFile Config;

        public bool IsRunningOnLinux { get => RuntimeInformation.IsOSPlatform(OSPlatform.Linux); }
        public bool IsRunningOnMac { get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX); }

        public bool PlaytimeLogging
        {
            get => Config?.PlaybackTracking ?? false;
            set
            {
                Config.PlaybackTracking = value;
            }
        }
        public bool IntegrateDiscordRPC
        {
            get => Config?.IntegrateDiscordRPC ?? false;
            set
            {
                Config.IntegrateDiscordRPC = value;
            }
        }
        public bool IntegrateMPRIS
        {
            get => Config?.IntegrateMPRIS ?? false;
            set
            {
                Config.IntegrateMPRIS = value;
            }
        }
        public bool MPRISShowCoverArt
        {
            get => Config?.MPRISShowCoverArt ?? false;
            set
            {
                Config.MPRISShowCoverArt = value;
            }
        }
        public bool CheckForUpdates
        {
            get => Config?.CheckForUpdates ?? false;
            set
            {
                Config.CheckForUpdates = value;
            }
        }

        public SettingsViewModel()
        {

        }

        public void StartThings()
        {
            this.RaisePropertyChanged(nameof(PlaytimeLogging));
            this.RaisePropertyChanged(nameof(IntegrateDiscordRPC));
            this.RaisePropertyChanged(nameof(IntegrateMPRIS));
            this.RaisePropertyChanged(nameof(MPRISShowCoverArt));
            this.RaisePropertyChanged(nameof(CheckForUpdates));
        }

        public void ReportIssueCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer/issues/new");
        public void ViewSourceCodeCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer");
        public void ViewLicenseCommand() => InterfaceUtils.OpenURL(@"https://choosealicense.com/licenses/gpl-3.0/");
        public void ViewWebsiteCommand() => InterfaceUtils.OpenURL(@"https://royce551.github.io/FRESHMusicPlayer");
    }
}
