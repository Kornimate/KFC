using KFC.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KFC.View {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static (double w, double h) WindowSize = (1400, 850);
        private static bool InGame;
        private static bool InMenu;
        private static bool InCredits;
        private static bool HotKeysDisabled;

        private static readonly MediaPlayer mediaPlayer = new() { Volume = 0.1 };
        private static bool MusicEnabled;
        private static readonly Random random = new();
        private static CancellationTokenSource? tokenSource;
        private static readonly List<string> tracks = new()
        {
            "A Positive Direction",
            "City Grey",
            "Media",
            "Option",
            "Afterlife",
            "One",
            "Future Club"
        };
        private readonly Configuration configuration;

        #region AnimationEvents
        private event EventHandler? MenuHidden;
        private event EventHandler? MenuShown;
        private event EventHandler? CreditsHidden;
        private event EventHandler? CreditsShown;

        /// <summary>
        /// Fires when the Application hid the game area.
        /// </summary>
        public event EventHandler? GameHidden;

        /// <summary>
        /// Fires when the Application loaded the TileMatrix in.
        /// </summary>
        public event EventHandler? TileMatrixShown;

        /// <summary>
        /// Fires when the Save Menu has been showed.
        /// </summary>
        public event EventHandler? SaveMenuShowed;

        /// <summary>
        /// Fires when the Finance Menu has been showed.
        /// </summary>
        public event EventHandler? FinanceMenuShowed;

        /// <summary>
        /// Fires when the Finance Menu has been closed.
        /// </summary>
        public event EventHandler? FinanceMenuHidden;
        #endregion

        #region AnimationEventHandlers
        private readonly EventHandler showGameAreaHandler;
        private readonly EventHandler showCreditsHandler;
        private readonly EventHandler showMainMenuHandler;
        #endregion

        #region AnimationConstructors
        private static DoubleAnimation A_double(double from = -1, double to = 0, int begin = 0, int durationInMs = 700)
        {
            if(from == -1)
                return new()
                {
                    Duration = new TimeSpan(0, 0, 0, 0, durationInMs),
                    BeginTime = new TimeSpan(0, 0, 0, 0, begin),
                    To = to,
                    EasingFunction = new PowerEase()
                    {
                        Power = 4,
                        EasingMode = EasingMode.EaseOut
                    }
                };

            return new()
            {
                Duration = new TimeSpan(0, 0, 0, 0, durationInMs),
                BeginTime = new TimeSpan(0, 0, 0, 0, begin),
                From = from,
                To = to,
                EasingFunction = new PowerEase()
                {
                    Power = 4,
                    EasingMode = EasingMode.EaseOut
                }
            };
        }
        private static ThicknessAnimation A_margin(int top = 0, int left = 0, int right = 0, int bottom = 0)
        {
            return new()
            {
                Duration = new TimeSpan(0, 0, 0, 0, 700),
                To = new Thickness()
                {
                    Top = top,
                    Left = left,
                    Bottom = bottom,
                    Right = right
                },
                EasingFunction = new PowerEase()
                {
                    Power = 4,
                    EasingMode = EasingMode.EaseOut
                }
            };
        }
        #endregion

        /// <summary>
        /// Initializes a new MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _window.Height = WindowSize.h;
            _window.Width = WindowSize.w;
            _windowBorder.Height = WindowSize.h + 10;
            _windowBorder.Width = WindowSize.w + 10;

            showMainMenuHandler = (_, _) => ShowMainMenu();
            showGameAreaHandler = (_, _) => ShowGameArea();
            showCreditsHandler = (_, _) => ShowCredits();

            InMenu = true;
            MenuShown += (_, _) => InMenu = true;
            MenuHidden += (_, _) => InMenu = false;

            InCredits = false;
            CreditsShown += (_, _) => InCredits = true;
            CreditsHidden += showMainMenuHandler;

            InGame = false;
            TileMatrixShown += (_, _) => InGame = true;
            GameHidden += showMainMenuHandler;

            HotKeysDisabled = false;

            MusicEnabled = ConfigurationManager.AppSettings["Music"] == "On";
            _musicBtn.Label = MusicEnabled ? "Music: On" : "Music: Off";
            mediaPlayer.MediaEnded += async (_, _) => await DelayBetweenTracks(10);
            CanPlayTrackEvent += (_, _) => PlayTrack(SelectTrack());
            tokenSource = new();

            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        #region Interactions
        /// <summary>
        /// Starts the game from the Menu.
        /// </summary>
        public void StartGame()
        {
            InMenu = false;

            MenuHidden -= showCreditsHandler;
            MenuHidden -= showGameAreaHandler;
            MenuHidden += showGameAreaHandler;

            HideMainMenu();
            HidePopUp();

            if (!MusicEnabled) return;

            tokenSource = new();
            _ = DelayBetweenTracks(1);
            
        }
        private void StartGameClick(object sender, RoutedEventArgs e) => StartGame();
        private void LeaveGame(object sender, RoutedEventArgs e)
        {
            InGame = false;
            mediaPlayer.Stop();
            tokenSource!.Cancel();
            _musicTrack.IsPlaying = false;

            HideSaveMenu();
            HideGameOverMenu();
            HideHelp(this, new());
            HidePopUp();
            HideGameArea();
        }
        private void ToCredits(object sender, RoutedEventArgs e)
        {
            InMenu = false;

            MenuHidden -= showGameAreaHandler;
            MenuHidden -= showCreditsHandler;
            MenuHidden += showCreditsHandler;

            HideMainMenu();

        }
        private void LeaveCredits(object sender, RoutedEventArgs e)
        {
            InCredits = false;
            HideCredits();
        }
        /// <summary>
        /// Plays the closing animation and closes the game.
        /// </summary>
        public void ExitGame()
        {
            DoubleAnimation anim = A_double(from: _window.ActualHeight);
            anim.Completed += (_, _) => Application.Current.Shutdown();

            _windowBorder
                .BeginAnimation(HeightProperty, A_double(from: _windowBorder.ActualHeight));

            _window
                .BeginAnimation(HeightProperty, anim);
        }
        private void ExitGameClick(object sender, RoutedEventArgs e) => ExitGame();
        private void Music_Switch(object sender, RoutedEventArgs e)
        {
            MusicEnabled = !MusicEnabled;
            _musicBtn.Label = MusicEnabled ? "Music: On" : "Music: Off";
            configuration.AppSettings.Settings["Music"].Value = MusicEnabled ? "On" : "Off";
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

        #region Animations
        private void ShowMainMenu()
        {
            _mainMenu.Visibility = Visibility.Visible;

            DoubleAnimation showBg = A_double(from: 0, to: 1);
            showBg.Completed += (_, _) =>
            {
                MenuShown?.Invoke(this, new());
                BindingOperations.ClearBinding(_matrix, ItemsControl.ItemsSourceProperty);
            };

            _mainMenu
                .BeginAnimation(MarginProperty, A_margin());

            _mainMenu_saveSlots
                .BeginAnimation(MarginProperty, A_margin(right: 20));

            _mainMenu
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));

            _background
                .BeginAnimation(OpacityProperty, showBg);
        }
        private void HideMainMenu()
        {
            ThicknessAnimation moveSaves = A_margin(right: -20);
            moveSaves.Completed += (_, _) =>
            {
                _mainMenu.Visibility = Visibility.Collapsed;
                MenuHidden?.Invoke(this, new());
            };

            _background
                .BeginAnimation(OpacityProperty, A_double());

            _mainMenu
                .BeginAnimation(OpacityProperty, A_double());

            _mainMenu
                .BeginAnimation(MarginProperty, A_margin(left: -500));

            _mainMenu_saveSlots
                .BeginAnimation(MarginProperty, moveSaves);
        }
        private void ShowGameTileTable()
        {
            _gameTileTable.Visibility = Visibility.Visible;
            DoubleAnimation loader = A_double(from: 0, to: 0, durationInMs: 1); //hide stutter

            loader.Completed += (_, _) =>
            {
                _loadingTitle.Visibility = Visibility.Collapsed;
                _loadingTitle.Opacity = 0;
                _gameTileTable.BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
                TileMatrixShown?.Invoke(this, new());
            };
            _matrix.SetBinding(ItemsControl.ItemsSourceProperty, "TileMatrix");

            _gameTileTable
                .BeginAnimation(OpacityProperty, loader);
        }
        private void HideGameTileTable()
        {
            _leftGameMenu
                .BeginAnimation(WidthProperty, A_double());

            _rightGameMenu
                .BeginAnimation(WidthProperty, A_double());

            _topGameMenuContainer
                .BeginAnimation(HeightProperty, A_double());

            _gameTileTable
                .BeginAnimation(OpacityProperty, A_double());

            _topGameMenu
                .BeginAnimation(OpacityProperty, A_double());

            _gameTileTable.Visibility = Visibility.Collapsed;
        }
        private void ShowGameArea()
        {
            double width = _window.Width / 5;
            DoubleAnimation setTopMenuHeightAnimation = A_double(from: 0, to: 60, begin: 500);
            setTopMenuHeightAnimation.Completed += (_, _) =>
            {
                _topGameMenu.Visibility = Visibility.Visible;
                _topGameMenu
                    .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));

                ShowGameTileTable();
            };

            _gameArea.Visibility = Visibility.Visible;
            _loadingTitle.Visibility = Visibility.Visible;

            _gameArea
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));

            _leftGameMenu
                .BeginAnimation(WidthProperty, A_double(from: 0, to: width - 20));

            _rightGameMenu
                .BeginAnimation(WidthProperty, A_double(from: 0, to: width - 20));

            _topGameMenuContainer
                .BeginAnimation(HeightProperty, setTopMenuHeightAnimation);

            _loadingTitle
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));


        }
        private void HideGameArea()
        {
            DoubleAnimation anim = A_double();
            anim.Completed += (_, _) =>
            {
                _gameArea.Visibility = Visibility.Collapsed;
                HideGameTileTable();
                GameHidden?.Invoke(this, new());
            };

            _gameArea
                .BeginAnimation(OpacityProperty, anim);
        }
        private void ShowCredits()
        {
            if (_credits.Visibility == Visibility.Visible) return;
            _credits.Visibility = Visibility.Visible;
            DoubleAnimation anim = A_double(from: 0, to: 1);
            anim.Completed += (_, _) => CreditsShown?.Invoke(this, new());

            _credits
                .BeginAnimation(OpacityProperty, anim);
        }
        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            HotKeysDisabled = true;
            if (_helpPanel.Visibility == Visibility.Visible) return;
            _helpPanel.Visibility = Visibility.Visible;

            _helpPanelInner
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
        }
        private void HideHelp(object sender, RoutedEventArgs e)
        {
            DoubleAnimation hide = A_double(from: 1, to: 0);
            hide.Completed += (_, _) =>
            {
                HotKeysDisabled = false;
                _helpPanel.Visibility = Visibility.Collapsed;
            };

            _helpPanelInner
                .BeginAnimation(OpacityProperty, hide);
        }
        private void HideCredits()
        {
            _credits.Visibility = Visibility.Collapsed;
            CreditsHidden?.Invoke(this, new());
        }
        private void ShowSaveMenu(object sender, RoutedEventArgs e)
        {
            HotKeysDisabled = true;
            SaveMenuShowed?.Invoke(this, EventArgs.Empty);
            if (_savePanel.Visibility == Visibility.Visible) return;
            _savePanel.Visibility = Visibility.Visible;

            _savePanelInner
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
        }
        private void ShowFinancesMenu(object sender, RoutedEventArgs e)
        {
            FinanceMenuShowed?.Invoke(this, EventArgs.Empty);
            if (_financesPanel.Visibility == Visibility.Visible) return;
            _financesPanel.Visibility = Visibility.Visible;

            _financesPanelInner
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
        }
        private void HideFinancesMenu(object sender, RoutedEventArgs e)
        {
            DoubleAnimation hide = A_double(from: 1, to: 0);
            hide.Completed += (_, _) => _financesPanel.Visibility = Visibility.Collapsed;

            _financesPanelInner
                .BeginAnimation(OpacityProperty, hide);
        }

        /// <summary>
        /// Hides the Save Panel in-game programmatically.
        /// </summary>
        public void HideSaveMenu()
        {
            DoubleAnimation hide = A_double(from: 1, to: 0);
            hide.Completed += (_, _) =>
            {
                HotKeysDisabled = false;
                _savePanel.Visibility = Visibility.Collapsed;
            };

            _savePanelInner
                .BeginAnimation(OpacityProperty, hide);
        }
        private void HideSaveMenuClick(object sender, RoutedEventArgs e) => HideSaveMenu();

        /// <summary>
        /// Shows the Game Over screen in-game programmatically.
        /// </summary>
        public void ShowGameOverMenu()
        {
            if (_gameOverPanel.Visibility == Visibility.Visible) return;
            _gameOverPanel.Visibility = Visibility.Visible;

            _gameOverPanelInner
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
        }

        /// <summary>
        /// Hides the Game Over screen in-game programmatically.
        /// </summary>
        public void HideGameOverMenu()
        {
            DoubleAnimation hide = A_double(from: 1, to: 0);
            hide.Completed += (_, _) => _gameOverPanel.Visibility = Visibility.Collapsed;

            _gameOverPanelInner
                .BeginAnimation(OpacityProperty, hide);
        }

        /// <summary>
        /// Shows the PopUp menu in the menu or in-game.
        /// </summary>
        public void ShowPopUp()
        {
            var popup = (InMenu || !InGame) ? _savePopUp : _popup;

            if (popup.Visibility == Visibility.Visible) return;
            popup.Visibility = Visibility.Visible;

            popup
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
            popup
                .BeginAnimation(MarginProperty, A_margin(top: 15, left: 15, right: 15, bottom: 75));
        }

        /// <summary>
        /// Hides both the in-game and menu PopUp.
        /// </summary>
        public void HidePopUp()
        {
            DoubleAnimation hideP = A_double(to: 0);
            hideP.Completed += (_, _) => _popup.Visibility = Visibility.Collapsed;

            _popup
                .BeginAnimation(OpacityProperty, hideP);

            _popup
                .BeginAnimation(MarginProperty, A_margin(top: 15, left: 15, right:15, bottom: 50));

            DoubleAnimation hideS = A_double(to: 0);
            hideS.Completed += (_, _) => _savePopUp.Visibility = Visibility.Collapsed;

            _savePopUp
                .BeginAnimation(OpacityProperty, hideS);

            _savePopUp
                .BeginAnimation(MarginProperty, A_margin(top: 15, left: 15, right: 15, bottom: 50));
        }
        private void HidePopUpClick(object sender, RoutedEventArgs e) => HidePopUp();

        /// <summary>
        /// Shows the Finances Menu in-game.
        /// </summary>
        public void ShowFinancesMenu()
        {
            FinanceMenuShowed?.Invoke(this, EventArgs.Empty);
            if (_financesPanel.Visibility == Visibility.Visible) return;
            _financesPanel.Visibility = Visibility.Visible;

            _financesPanelInner
                .BeginAnimation(OpacityProperty, A_double(from: 0, to: 1));
        }
        private void ShowFinancesMenuClick(object sender, RoutedEventArgs e) => ShowFinancesMenu();

        /// <summary>
        /// Hides the Finances Menu in-game.
        /// </summary>
        public void HideFinancesMenu()
        {
            FinanceMenuHidden?.Invoke(this, EventArgs.Empty);
            DoubleAnimation hide = A_double(from: 1, to: 0);
            hide.Completed += (_, _) => _financesPanel.Visibility = Visibility.Collapsed;

            _financesPanelInner
                .BeginAnimation(OpacityProperty, hide);
        }
        private void HideFinancesMenuClick(object sender, RoutedEventArgs e) => HideFinancesMenu();
        #endregion

        #region Music
        private static string? prevTrack = null;
        private string? SelectTrack()
        {
            if (!MusicEnabled) return null;

            int step = 5000; // 7*step ~a high number of population
            int currentPopulation = Math.Min(step * tracks.Count, Convert.ToInt32(_population.Text.Split(" ")[1]));
            

            int last = tracks.Count - 1;


            int rand = random.Next((int)Math.Pow(step * tracks.Count, 10.0/11));
            rand = (int)Math.Pow(rand,  11.0/10);

            int nonRandPosition = Math.Min(last,(currentPopulation / step) + 1);
            int randPosition = ((currentPopulation + rand) / step) + 1;

            if (randPosition - nonRandPosition >= 2)
                randPosition = Math.Max(0,((currentPopulation - rand) / step) + 1);

            randPosition =
                tracks[randPosition].Equals(prevTrack) ? randPosition + 1 : randPosition;

            if (randPosition < 0)
                randPosition = tracks[0].Equals(prevTrack) ? 1 : 0;

            if (randPosition > last)
                randPosition = tracks[last].Equals(prevTrack) ? last - 1 : last;

            prevTrack = tracks[randPosition];

            return tracks[randPosition];
        }

        private void PlayTrack(string? trackName)
        {
            if (!MusicEnabled || trackName == null) return;

            mediaPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "/Music/" + trackName + ".mp3"));

            mediaPlayer.Play();

            _musicTrack.TrackName = trackName;
            _musicTrack.IsPlaying = true;
        }


        private async Task DelayBetweenTracks(double seconds)
        {
            if (!MusicEnabled) return;

            _musicTrack.IsPlaying = false;

            int delayUntilNextTrack = random.Next((int)seconds*1000, (int)seconds*10000);

            try
            {
                await Task.Delay(delayUntilNextTrack, tokenSource!.Token);
                CanPlayTrackEvent?.Invoke(this, new());
            }
            catch (TaskCanceledException)
            {
                //is thrown when delay is canceled --> when leaving the game
            }
        }

        private event EventHandler? CanPlayTrackEvent;

        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab) e.Handled = true;
            if (e.Key == Key.Escape && InMenu) ExitGame();
            if (e.Key == Key.Escape && InGame) LeaveGame(this, new());
            if (e.Key == Key.Escape && InCredits) LeaveCredits(this, new());
            if (e.Key == Key.U && !HotKeysDisabled)
            {
                UpgradeRequestEvent?.Invoke(this, new());
            }
            if (e.Key == Key.R && !HotKeysDisabled)
            {
                RepairRequestEvent?.Invoke(this, new());
            }
        }
        public event EventHandler? UpgradeRequestEvent;
        public event EventHandler? RepairRequestEvent;
        private void MoveWindow(object? sender, MouseButtonEventArgs e) => DragMove();
        private void TaxSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            string name = (sender as TaxSlider)!.Label;
            TaxSliderReleasedEvent?.Invoke(this, name.Split(' ')[0]);
        }

        /// <summary>
        /// Fires when the Mouse is released from a Tax Slider.
        /// </summary>
        public event EventHandler<string>? TaxSliderReleasedEvent;

        /// <summary>
        /// Launches the browser to show the Leaderboard.
        /// </summary>
        public static void LaunchBrowser()
        {
            try
            {
                Process.Start(new ProcessStartInfo() 
                { 
                    FileName = ConfigurationManager.AppSettings["URL"], 
                    UseShellExecute = true 
                });
            }
            catch (Exception)
            {
                //just do nothing
            }
        }
        private void LaunchBrowserClick(object sender, RoutedEventArgs e) => LaunchBrowser();
    }
}
