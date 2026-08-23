using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiSkillTester.Models;
using MauiSkillTester.Services;

namespace MauiSkillTester.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;

    private Playlist? _selectedPlaylist;
    private string _newPlaylistName = string.Empty;
    private string _newPlaylistDescription = string.Empty;

    private string _newVideoTitle = string.Empty;
    private string _newVideoUrl = string.Empty;
    private string _newVideoChannel = string.Empty;
    private int _newVideoDuration;
    private string _newVideoNotes = string.Empty;

    private string _searchQuery = string.Empty;
    private bool _showOnlyUnwatched;
    private PlaylistStats? _currentPlaylistStats;

    public ObservableCollection<Playlist> Playlists { get; } = new();
    public ObservableCollection<VideoItem> Videos { get; } = new();

    #region תכונות עבור Binding למסך (Properties)

    public PlaylistStats? CurrentPlaylistStats
    {
        get => _currentPlaylistStats;
        set
        {
            if (_currentPlaylistStats != value)
            {
                _currentPlaylistStats = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStats));
            }
        }
    }

    public bool HasStats => CurrentPlaylistStats != null && CurrentPlaylistStats.TotalVideos > 0;

    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (_selectedPlaylist != value)
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedPlaylist));
                ((Command)AddVideoCommand).ChangeCanExecute();

                // ברגע שנבחר פלייליסט חדש - נטען אוטומטית את הסרטונים שלו
                if (_selectedPlaylist != null)
                {
                    _ = LoadVideosForSelectedPlaylistAsync();
                }
            }
        }
    }

    public bool HasSelectedPlaylist => SelectedPlaylist != null;

    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set
        {
            if (_newPlaylistName != value)
            {
                _newPlaylistName = value;
                OnPropertyChanged();
                ((Command)AddPlaylistCommand).ChangeCanExecute();
            }
        }
    }

    public string NewPlaylistDescription
    {
        get => _newPlaylistDescription;
        set
        {
            if (_newPlaylistDescription != value)
            {
                _newPlaylistDescription = value;
                OnPropertyChanged();
            }
        }
    }

    public string NewVideoTitle
    {
        get => _newVideoTitle;
        set
        {
            if (_newVideoTitle != value)
            {
                _newVideoTitle = value;
                OnPropertyChanged();
                ((Command)AddVideoCommand).ChangeCanExecute();
            }
        }
    }

    public string NewVideoUrl
    {
        get => _newVideoUrl;
        set
        {
            if (_newVideoUrl != value)
            {
                _newVideoUrl = value;
                OnPropertyChanged();
                ((Command)AddVideoCommand).ChangeCanExecute();
            }
        }
    }

    public string NewVideoChannel
    {
        get => _newVideoChannel;
        set
        {
            if (_newVideoChannel != value)
            {
                _newVideoChannel = value;
                OnPropertyChanged();
            }
        }
    }

    public int NewVideoDuration
    {
        get => _newVideoDuration;
        set
        {
            if (_newVideoDuration != value)
            {
                _newVideoDuration = value;
                OnPropertyChanged();
            }
        }
    }

    public string NewVideoNotes
    {
        get => _newVideoNotes;
        set
        {
            if (_newVideoNotes != value)
            {
                _newVideoNotes = value;
                OnPropertyChanged();
            }
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged();
                _ = LoadVideosForSelectedPlaylistAsync();
            }
        }
    }

    public bool ShowOnlyUnwatched
    {
        get => _showOnlyUnwatched;
        set
        {
            if (_showOnlyUnwatched != value)
            {
                _showOnlyUnwatched = value;
                OnPropertyChanged();
                _ = LoadVideosForSelectedPlaylistAsync();
            }
        }
    }

    public List<VideoSortOptionItem> AvailableSortOptions { get; } = VideoSortOptionItem.AllOptions;

    private VideoSortOptionItem? _selectedSortOptionItem;
    public VideoSortOptionItem? SelectedSortOptionItem
    {
        get => _selectedSortOptionItem;
        set
        {
            if (_selectedSortOptionItem != value)
            {
                _selectedSortOptionItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedSortOption));
                _ = LoadVideosForSelectedPlaylistAsync();
            }
        }
    }

    public VideoSortOption SelectedSortOption => SelectedSortOptionItem?.Option ?? VideoSortOption.NewestFirst;

    #endregion

    #region פקודות (Commands)

    public ICommand LoadDataCommand { get; }
    public ICommand AddPlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand AddVideoCommand { get; }
    public ICommand DeleteVideoCommand { get; }
    public ICommand ToggleWatchedCommand { get; }
    public ICommand OpenVideoUrlCommand { get; }

    #endregion

    public MainViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "ניהול סרטוני YouTube & פלייליסטים";
        _selectedSortOptionItem = AvailableSortOptions.Count > 0 ? AvailableSortOptions[0] : null;

        LoadDataCommand = new Command(async () => await LoadDataAsync());

        AddPlaylistCommand = new Command(
            execute: async () => await AddPlaylistAsync(),
            canExecute: () => !string.IsNullOrWhiteSpace(NewPlaylistName)
        );

        DeletePlaylistCommand = new Command<Playlist>(async (playlist) => await DeletePlaylistAsync(playlist));

        AddVideoCommand = new Command(
            execute: async () => await AddVideoAsync(),
            canExecute: () => SelectedPlaylist != null &&
                              !string.IsNullOrWhiteSpace(NewVideoTitle) &&
                              !string.IsNullOrWhiteSpace(NewVideoUrl)
        );

        DeleteVideoCommand = new Command<VideoItem>(async (video) => await DeleteVideoAsync(video));
        ToggleWatchedCommand = new Command<VideoItem>(async (video) => await ToggleWatchedAsync(video));
        OpenVideoUrlCommand = new Command<VideoItem>(async (video) => await OpenVideoUrlAsync(video));
    }

    #region מימוש הפעולות (Action Methods)

    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            Playlists.Clear();

            var playlistsFromDb = await _databaseService.GetPlaylistsAsync();
            foreach (var p in playlistsFromDb)
            {
                Playlists.Add(p);
            }

            if (Playlists.Count > 0)
            {
                _selectedPlaylist = Playlists[0];
                OnPropertyChanged(nameof(SelectedPlaylist));
                OnPropertyChanged(nameof(HasSelectedPlaylist));
                ((Command)AddVideoCommand).ChangeCanExecute();

                await LoadVideosForSelectedPlaylistAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("שגיאה בטעינת נתונים", ex.Message, "אישור");
        }
        finally
        {
            IsLoading = false;
        }
    }


    public async Task LoadVideosForSelectedPlaylistAsync()
    {
        if (SelectedPlaylist == null)
        {
            Videos.Clear();
            CurrentPlaylistStats = null;
            return;
        }

        try
        {
            IsLoading = true;
            Videos.Clear();

            var videosFromDb = await _databaseService.GetFilteredVideosAsync(
                SelectedPlaylist.Id,
                SearchQuery,
                ShowOnlyUnwatched,
                SelectedSortOption);

            foreach (var v in videosFromDb)
            {
                Videos.Add(v);
            }

            await RefreshStatsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshStatsAsync()
    {
        if (SelectedPlaylist != null)
        {
            CurrentPlaylistStats = await _databaseService.GetPlaylistStatsAsync(SelectedPlaylist.Id);
        }
        else
        {
            CurrentPlaylistStats = null;
        }
    }

    private async Task AddPlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPlaylistName)) return;

        var playlist = new Playlist
        {
            Name = NewPlaylistName.Trim(),
            Description = NewPlaylistDescription.Trim(),
            CreatedAt = DateTime.Now
        };

        await _databaseService.SavePlaylistAsync(playlist);
        Playlists.Insert(0, playlist);
        SelectedPlaylist = playlist;

        NewPlaylistName = string.Empty;
        NewPlaylistDescription = string.Empty;
    }

    private async Task DeletePlaylistAsync(Playlist? playlist)
    {
        if (playlist == null) return;

        bool confirm = await ShowConfirmAsync(
            "מחיקת פלייליסט",
            $"האם אתה בטוח שברצונך למחוק את הפלייליסט '{playlist.Name}' ואת כל הסרטונים שבו?",
            "כן, מחק",
            "ביטול");

        if (!confirm) return;

        await _databaseService.DeletePlaylistAsync(playlist);
        Playlists.Remove(playlist);

        if (SelectedPlaylist == playlist)
        {
            SelectedPlaylist = Playlists.Count > 0 ? Playlists[0] : null;
        }
        await RefreshStatsAsync();
    }

    private async Task AddVideoAsync()
    {
        if (SelectedPlaylist == null || string.IsNullOrWhiteSpace(NewVideoTitle) || string.IsNullOrWhiteSpace(NewVideoUrl))
            return;

        var video = new VideoItem
        {
            PlaylistId = SelectedPlaylist.Id,
            Title = NewVideoTitle.Trim(),
            Url = NewVideoUrl.Trim(),
            ChannelName = string.IsNullOrWhiteSpace(NewVideoChannel) ? "לא צוין" : NewVideoChannel.Trim(),
            DurationMinutes = NewVideoDuration > 0 ? NewVideoDuration : 5,
            IsWatched = false,
            Notes = NewVideoNotes.Trim(),
            CreatedAt = DateTime.Now
        };

        await _databaseService.SaveVideoAsync(video);
        Videos.Insert(0, video);
        await RefreshStatsAsync();

        // איפוס שדות הטופס
        NewVideoTitle = string.Empty;
        NewVideoUrl = string.Empty;
        NewVideoChannel = string.Empty;
        NewVideoDuration = 0;
        NewVideoNotes = string.Empty;
    }

    private async Task DeleteVideoAsync(VideoItem? video)
    {
        if (video == null) return;

        await _databaseService.DeleteVideoAsync(video);
        Videos.Remove(video);
        await RefreshStatsAsync();
    }

    private async Task ToggleWatchedAsync(VideoItem? video)
    {
        if (video == null) return;

        await _databaseService.ToggleWatchedAsync(video);
        // רענון ברשימה
        int index = Videos.IndexOf(video);
        if (index >= 0)
        {
            Videos[index] = video;
        }
        await RefreshStatsAsync();
    }

    private async Task OpenVideoUrlAsync(VideoItem? video)
    {
        if (video == null || string.IsNullOrWhiteSpace(video.Url)) return;

        try
        {
            Uri uri = new Uri(video.Url);
            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("שגיאה בפתיחת קישור", $"לא ניתן לפתוח את הקישור: {ex.Message}", "אישור");
        }
    }

    #endregion

    #region עוזרי הודעות (Dialog Helpers)

    private static async Task ShowAlertAsync(string title, string message, string cancel)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync(title, message, cancel);
        }
        else if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync(title, message, cancel);
        }
    }

    private static async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        if (Shell.Current != null)
        {
            return await Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
        }
        if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
        {
            return await Application.Current.Windows[0].Page!.DisplayAlertAsync(title, message, accept, cancel);
        }
        return true;
    }

    #endregion

}

