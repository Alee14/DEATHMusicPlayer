﻿using ATL;
using FRESHMusicPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using ATL.Playlist;
using FRESHMusicPlayer.Handlers.Notifications;
using System.IO;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for QueueManagementPage.xaml
    /// </summary>
    public partial class QueueManagement : Page
    {
        public QueueManagement()
        {
            InitializeComponent();
            PopulateList();
            MainWindow.Player.SongChanged += Player_SongChanged;
        }
        
        public void PopulateList()
        {
            var list = MainWindow.Player.Queue;
            var nextlength = 0;
            int number = 1;
            foreach (var song in list)
            {
                string place;
                if (MainWindow.Player.QueuePosition == number) place = "Playing:";
                else if (MainWindow.Player.QueuePosition == number - 1) place = "Next:";
                else place = (number - MainWindow.Player.QueuePosition).ToString();
                Track track = new Track(song);
                QueueList.Items.Add(new QueueEntry(track.Artist, track.Album, track.Title, place, number - 1));
                if (MainWindow.Player.QueuePosition < number) nextlength += track.Duration;
                number++;
            }
            RemainingTimeLabel.Text = $"Remaining Time - {new TimeSpan(0,0,0,nextlength):mm\\:ss}";
        }
        
        private void Player_SongChanged(object sender, EventArgs e)
        {
            QueueList.Items.Clear();
            PopulateList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.SongChanged -= Player_SongChanged;
            QueueList.Items.Clear();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                MainWindow.Player.AddQueue(dialog.FileName);
                QueueList.Items.Clear();
                PopulateList();
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Playlist Files|*.xspf;*.asx;*.wax;*.wvx;*.b4s;*.m3u;*.m3u8;*.pls;*.smil;*.smi;*.zpl;";
            if (dialog.ShowDialog() == true)
            {
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(dialog.FileName);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                    {
                        MainWindow.NotificationHandler.Add(new NotificationBox(new NotificationInfo("Missing file",
                                                                                                    $"One of the tracks in the playlist was not added because it could not be found.\nMissing File: {s}",
                                                                                                    true,
                                                                                                    true)));
                        continue;
                    }
                    MainWindow.Player.AddQueue(s);
                    QueueList.Items.Clear();
                    PopulateList();
                }
            }
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.ClearQueue();
            QueueList.Items.Clear();
            PopulateList();
        }
    }
}