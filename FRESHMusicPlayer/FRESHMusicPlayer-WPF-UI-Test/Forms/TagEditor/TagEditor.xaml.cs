﻿using ATL;
using FRESHMusicPlayer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using Winforms = System.Windows.Forms;
using System.Windows.Shapes;
using FRESHMusicPlayer.Forms.TagEditor.Integrations;

namespace FRESHMusicPlayer.Forms.TagEditor
{
    /// <summary>
    /// Interaction logic for TagEditor.xaml
    /// </summary>
    public partial class TagEditor : Window
    {
        public List<string> FilePaths = new List<string>();     
        private List<string> filePathsToSaveInBackground = new List<string>();
        List<string> Displayfilepaths = new List<string>();
        private bool unsavedChanges = false;
        private HttpClient httpClient = new HttpClient();
        public TagEditor(List<string> filePaths)
        {
            InitializeComponent();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FRESHMusicPlayer/8.2.0 (https://github.com/Royce551/FRESHMusicPlayer)");
            FilePaths = filePaths;
            MainWindow.Player.SongChanged += Player_SongChanged;
            InitFields();
        }

        public void InitFields()
        {
            unsavedChanges = true; // hack in order to make the TextChanged handler not go off          
            int iterations = 1;           
            foreach (string path in FilePaths)
            {
                Track track = new Track(path);
                ArtistBox.Text = track.Artist;
                TitleBox.Text = track.Title;
                AlbumBox.Text = track.Album;
                GenreBox.Text = track.Genre;
                YearBox.Text = track.Year.ToString();

                AlbumArtistBox.Text = track.AlbumArtist;
                ComposerBox.Text = track.Composer;
                TrackNumBox.Text = track.TrackNumber.ToString();
                DiscNumBox.Text = track.DiscNumber.ToString();
                Displayfilepaths.Add(System.IO.Path.GetFileName(path));
                iterations++;
            }
            if (iterations <= 5) EditingHeader.Text = Properties.Resources.TAGEDITOR_EDITINGHEADER + string.Join(", ", Displayfilepaths);
            else EditingHeader.Text = Properties.Resources.TAGEDITOR_EDITINGHEADER + string.Join(", ", Displayfilepaths.Take(5)) + " + " + (Displayfilepaths.Count - 4);
            Title = $"{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            unsavedChanges = false;
        }

        public void SaveChanges(List<string> filePaths)
        {
            foreach (string path in filePaths)
            {
                Track track = new Track(path);
                track.Artist = ArtistBox.Text;
                track.Title = TitleBox.Text;
                track.Album = AlbumBox.Text;
                track.Genre = GenreBox.Text;
                track.Year = Convert.ToInt32(YearBox.Text);
                track.AlbumArtist = AlbumArtistBox.Text;
                track.Composer = ComposerBox.Text;
                track.TrackNumber = Convert.ToInt32(TrackNumBox.Text);
                track.DiscNumber = Convert.ToInt32(DiscNumBox.Text);
                track.Save();
            }
        }

        public void SaveButton()
        {
            unsavedChanges = false;
            Title = $"{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            foreach (string path in FilePaths)
            {
                if (path != MainWindow.Player.FilePath) continue; // We're good
                else
                {
                    filePathsToSaveInBackground.AddRange(FilePaths); // SongChanged event handler will handle this
                    BackgroundSaveIndicator.Visibility = Visibility.Visible;
                    return;
                }
            }
            SaveChanges(FilePaths);
        }

        public void ChangeFiles()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                FilePaths = dialog.FileNames.ToList();
                InitFields();
            }
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            if (filePathsToSaveInBackground.Count != 0)
            {
                foreach (string path in filePathsToSaveInBackground)
                {
                    if (path == MainWindow.Player.FilePath) break; // still listening to files that can't be properly saved
                }
                SaveChanges(filePathsToSaveInBackground);
                filePathsToSaveInBackground.Clear();
                Close();
            }            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (unsavedChanges == true)
            {
                MessageBoxResult result = MessageBox.Show(string.Format(Properties.Resources.TAGEDITOR_SAVECHANGES, string.Join(", ", Displayfilepaths)),
                                          "FRESHMusicPlayer Tag Editor",
                                          MessageBoxButton.YesNoCancel,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) SaveButton();
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else return;
            }
            if (filePathsToSaveInBackground.Count != 0) // this logic is needed because you cannot save tags to a file that the player is currently playing
            {
                Hide(); // window will later close itself in the songchanged handler once all necessary files have been saved
                e.Cancel = true;   
            }
            else
            {
                MainWindow.Player.SongChanged -= Player_SongChanged;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => SaveButton();

        private void Button_Click_1(object sender, RoutedEventArgs e) => ChangeFiles();

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!unsavedChanges)
            {
                unsavedChanges = true;
                Title = $"*{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            }
        }

        private void NewWindowItem_MouseDown(object sender, RoutedEventArgs e)
        {
            TagEditor tagEditor = new TagEditor(FilePaths);
            tagEditor.Show();
        }

        private void OpenMenu_MouseDown(object sender, RoutedEventArgs e) => ChangeFiles();

        private void SaveMenuItem(object sender, RoutedEventArgs e) => SaveButton();

        private void ExitMenuItem(object sender, RoutedEventArgs e) => Close();

        private void Window_DragEnter(object sender, DragEventArgs e) => e.Effects = DragDropEffects.Copy;

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop);
            FilePaths = tracks.ToList();
            InitFields();
        }

        private void DiscogsSourceMenuItem_Click(object sender, RoutedEventArgs e) => OpenAlbumIntegration(new DiscogsIntegration(httpClient));
        private void MusicBrainzSourceMenuItem_Click(object sender, RoutedEventArgs e) => OpenAlbumIntegration(new MusicBrainzIntegration(httpClient));
        private void OpenAlbumIntegration(IReleaseIntegration integration)
        {

            var dialog = new FMPTextEntryBox(Properties.Resources.TRACKINFO_ALBUM, AlbumBox.Text);
            dialog.ShowDialog();
            if (!dialog.OK) return;

            string query = dialog.Response;
            var results = integration.Search(query);

            var index = 0;
            if (!integration.Worked && integration.NeedsInternetConnection)
            {
                MessageBox.Show("You aren't connected to the internet :(", "FRESHMusicPlayer Tag Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (results.Count == 0 | !integration.Worked)
            {
                MessageBox.Show("No results were found for this album :(", "FRESHMusicPlayer Tag Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //if (results.Count > 1)
            //{
                var disambiguation = new IntegrationDisambiguation(results);
                disambiguation.ShowDialog();
                if (disambiguation.OK) index = disambiguation.SelectedIndex;
                else return;
           // }

            var filePath = FilePaths[0];
            var release = integration.Fetch(results[index].Id);
            var editor = new ReleaseIntegrationPage(release, new Track(filePath), filePath);
            editor.ShowDialog();
            if (editor.OK)
            {
                ArtistBox.Text = editor.TrackToSave.Artist;
                TitleBox.Text = editor.TrackToSave.Title;
                AlbumBox.Text = editor.TrackToSave.Album;
                GenreBox.Text = editor.TrackToSave.Genre;
                YearBox.Text = editor.TrackToSave.Year.ToString();
                TrackNumBox.Text = editor.TrackToSave.TrackNumber.ToString();
            }
        }

    }
}
