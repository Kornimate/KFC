using KFC.View;
using KFC.ViewModel;
using KFCSharedData;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace KFC {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameViewModelKFC? viewmodel;
        private Model.Application? gamemodel;
        private MainWindow? view;
        private HttpClient? client;
        private Random? rand;
        private byte[]? screenShot;

        private Timer? timer;

        private int showcounter = -1;
        public App()
        {
            Startup += new StartupEventHandler(AppStartUp);
        }

        private void AppStartUp(object sender, StartupEventArgs e)
        {
            view = new MainWindow();
            gamemodel = new Model.Application();
            viewmodel = new GameViewModelKFC(gamemodel);

            ConfigureClient();
            rand = new Random();

            viewmodel.callAppToLoadGame += new EventHandler<string>(LoadGame);
            viewmodel.callAppToSaveGame += new EventHandler<bool>(SaveGame);
            viewmodel.callAppToNewGame += new EventHandler(NewGame);
            viewmodel.callAppToDeleteGame += new EventHandler<string>(DeleteGame);
            viewmodel.callAppToLoadSuccess += new EventHandler(SaveFileLoaded);
            viewmodel.callAppToExitGame += new EventHandler(ExitGame);
            viewmodel.callAppToShowCredits += new EventHandler(ShowCredits);
            viewmodel.callAppToShowHelp += new EventHandler(ShowHelp);
            viewmodel.callAppToShowInfo += new EventHandler<string>(ShowInfo);
            viewmodel.callAppToShowGameOver += new EventHandler<string>(ShowGameOver);

            view.DataContext = viewmodel;
            view.TileMatrixShown += new EventHandler(StartTimer);
            view.TaxSliderReleasedEvent += new EventHandler<string>(SetTaxRate);
            view.GameHidden += new EventHandler(BackToMainMenu);
            view.SaveMenuShowed += new EventHandler(NeedForScreenShot);
            view.UpgradeRequestEvent += (_, _) => viewmodel.UpgradeCurrentTile();
            view.RepairRequestEvent += (_, _) => viewmodel.RepairCurrentTile();
            view.FinanceMenuShowed += new EventHandler(ViewModelCallOnPropertyChanged);

            view.Closing += new CancelEventHandler(AppClosing);
            view.Show();

            timer = new Timer();
            timer.Elapsed += ShowTimeChanged;
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Start();
        }

        private void ViewModelCallOnPropertyChanged(object? sender, EventArgs e)
        {
            viewmodel!.FinancesPropertyChanged();
        }

        private void NeedForScreenShot(object? sender, EventArgs e)
        {
            screenShot = TakeFullScreen();
        }

        private void ConfigureClient()
        {
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["APIURL"]!);
            }
            catch (Exception)
            {
                //just do nothing
            }
        }

        private void ShowTimeChanged(object? sender, ElapsedEventArgs e)
        {
            if (showcounter == -1) return;
            if (showcounter++ >= 3)
            {
                showcounter = -1;
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        view!.HidePopUp();
                    });
                }
                catch (TaskCanceledException)
                { 
                    //cancelled
                }

            }

        }

        private void ShowGameOver(object? sender, string e)
        {
            Dispatcher.BeginInvoke(() => view!.ShowGameOverMenu()); 
        }

        private void SetTaxRate(object? sender, string zone)
        {
            viewmodel!.SetTaxRate(zone);
        }

        private void BackToMainMenu(object? sender, EventArgs e)
        {
            viewmodel!.BackToMainMenu();
            view!.HidePopUp();
        }

        private void ShowInfo(object? sender, string message)
        {
            view!.ShowPopUp();
            showcounter = 0;
        }

        private void StartTimer(object? sender, EventArgs e)
        {
            viewmodel!.StartTimer();
        }

        private void ShowHelp(object? sender, EventArgs e)
        {
        }

        private void ShowCredits(object? sender, EventArgs e)
        {
            MessageBox.Show(view, "Credits:\nDinya Gergely: Best Model developer\nHorváth Ádám: Master of the View\nSebők Mátyás: Persistence Expert\nKornidesz Máté: The worst part of MVVM", "Credits");
        }

        private void ExitGame(object? sender, EventArgs e)
        {
            view!.ExitGame();
        }

        private void NewGame(object? sender, EventArgs e)
        {
            viewmodel!.StartNewGame();
        }

        private void SaveGame(object? sender, bool overwrite)
        {
            PostToServer();
            if (overwrite)
            {
                view!.HideSaveMenu();
                viewmodel!.OverwriteToPersistence();
            }
            else
            {
                view!.HideSaveMenu();
                viewmodel!.SaveGameToPersistence();
            }
        }

        private void LoadGame(object? sender, string path)
        {
            viewmodel!.LoadGameFromPersistence(path);
        }

        private void SaveFileLoaded(object? sender, EventArgs e) {
            Dispatcher.BeginInvoke(() => view!.StartGame());
        }
        
        private void DeleteGame(object? sender, string path)
        {
            viewmodel!.DeleteSaveFromPersistence(path);
        }

        private void AppClosing(object? sender, CancelEventArgs e)
        {
            client!.Dispose();
        }

        private string? GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
        }
        private byte[] TakeFullScreen()
        {
            double? screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;

            using (Bitmap bmp = new Bitmap((int)screenWidth,
                (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                    using var memoryStream = new MemoryStream();
                    bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    var byteArray = memoryStream.ToArray();
                    return byteArray;
                }
            }
        }

        private async void PostToServer()
        {
            try
            {
                int randNum = rand!.Next();
                string? mac = GetMacAddress();
                string picture = Convert.ToBase64String(screenShot!);
                var body = new RecordDTO { Address = mac + '.' + randNum.ToString() + '.' + Convert.ToBase64String(BitConverter.GetBytes(randNum)), Picture = picture, Name = viewmodel!.SaveName, Date = viewmodel!.Date, Population = viewmodel!.Population };
                await client!.PostAsJsonAsync(client!.BaseAddress, body);
            }
            catch
            {
                // when cancellationtoken is cancelled
            }
        }
    }
}
