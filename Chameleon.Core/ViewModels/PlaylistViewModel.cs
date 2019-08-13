﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Chameleon.Services.Services;
using MediaManager;
using MediaManager.Library;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Xamarin.Forms;

namespace Chameleon.Core.ViewModels
{
    public class PlaylistViewModel : BaseViewModel<IPlaylist>
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMediaManager _mediaManager;
        private readonly IPlaylistService _playlistService;

        public PlaylistViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService, IUserDialogs userDialogs, IMediaManager mediaManager, IPlaylistService playlistService) : base(logProvider, navigationService)
        {
            _userDialogs = userDialogs ?? throw new ArgumentNullException(nameof(userDialogs));
            _mediaManager = mediaManager ?? throw new ArgumentNullException(nameof(mediaManager));
            _playlistService = playlistService ?? throw new ArgumentNullException(nameof(playlistService));
        }

        private IMediaItem _selectedMediaItem;
        public IMediaItem SelectedMediaItem
        {
            get => _selectedMediaItem;
            set => SetProperty(ref _selectedMediaItem, value);
        }

        private IPlaylist _currentPlaylist = new Playlist();
        public IPlaylist CurrentPlaylist
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return _currentPlaylist;
                }
                else
                {
                    var searchedItems = _currentPlaylist.Where(x => x.Title.ToLower().Contains(SearchText.ToLower()) || x.Album.ToLower().Contains(SearchText.ToLower()));
                    var playlist = new Playlist();
                    foreach (var item in searchedItems)
                        playlist.Add(item);

                    return playlist;
                }
            }
            set => SetProperty(ref _currentPlaylist, value);
        }

        public bool IsVisible
        {
            get
            {
                return string.IsNullOrEmpty(SearchText);
            }
        }

        private FormattedString _trackAmount;
        public FormattedString TrackAmount
        {
            get => _trackAmount;
            set => SetProperty(ref _trackAmount, value);
        }

        private FormattedString _playlistTime;
        public FormattedString PlaylistTime
        {
            get => _playlistTime;
            set => SetProperty(ref _playlistTime, value);
        }

        private ImageSource _playPauseImage = ImageSource.FromFile("playback_controls_pause_button");
        public ImageSource PlayPauseImage
        {
            get => _playPauseImage;
            set => SetProperty(ref _playPauseImage, value);
        }

        private IMvxAsyncCommand _playerCommand;
        public IMvxAsyncCommand PlayerCommand => _playerCommand ?? (_playerCommand = new MvxAsyncCommand(PlayWhenSelected));

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                RaisePropertyChanged(nameof(CurrentPlaylist));
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        public override void Prepare(IPlaylist playlist)
        {
            playlist.Add(new MediaItem("http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4") { Title = "Item 1", Album = "Album 1" });
            playlist.Add(new MediaItem("http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4") { Title = "Item 2", Album = "Album 2" });
            CurrentPlaylist = playlist;

            var trackAmount = new FormattedString();
            trackAmount.Spans.Add(new Span { Text = CurrentPlaylist.Count.ToString(), FontAttributes = FontAttributes.Bold, FontSize = 12 });
            trackAmount.Spans.Add(new Span { Text = " tracks" });
            TrackAmount = trackAmount;

            var playlistTime = new FormattedString();
            playlistTime.Spans.Add(new Span { Text = CurrentPlaylist.TotalTime.Hours.ToString(), FontAttributes = FontAttributes.Bold, FontSize = 12 });
            playlistTime.Spans.Add(new Span { Text = " hours, "});
            playlistTime.Spans.Add(new Span { Text = CurrentPlaylist.TotalTime.Minutes.ToString(), FontAttributes = FontAttributes.Bold, FontSize = 12 });
            playlistTime.Spans.Add(new Span { Text = " minutes"});
            PlaylistTime = playlistTime;
        }

        private async Task PlayWhenSelected()
        {
            await NavigationService.Navigate<PlayerViewModel, IMediaItem>(SelectedMediaItem);
            SelectedMediaItem = null;
        }
    }
}
