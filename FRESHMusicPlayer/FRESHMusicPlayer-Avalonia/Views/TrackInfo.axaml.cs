using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System.ComponentModel;

namespace FRESHMusicPlayer.Views
{
    public partial class TrackInfo : UserControl
    {
        private TrackInfoViewModel ViewModel { get => DataContext as TrackInfoViewModel; }

        public TrackInfo()
        {
            InitializeComponent();
            DetachedFromLogicalTree += OnClosing;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public TrackInfo SetStuff(Player player)
        {
            ViewModel.Player = player;
            ViewModel.StartThings();
            return this;
        }

        private void OnClosing(object sender, LogicalTreeAttachmentEventArgs e)
        {
            ViewModel?.CloseThings();
        }
    }
}
