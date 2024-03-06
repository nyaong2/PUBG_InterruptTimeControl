using PUBG_InterruptTimeControl.Components;
using PUBG_InterruptTimeControl.Components.Modal;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Components.StartUp;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.PgReg;
using System.Net.Sockets;
using System.Net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using System.Threading;
using PUBG_InterruptTimeControl.Components.Modal.Warning;

namespace PUBG_InterruptTimeControl
{
    public partial class MainWindow : Window
    {
        private MsgService msgService;
        private DownloadService downloadService;
        private ProgramUtilService pgUtilService;
        private BlurEffect blur;

        private string nyaongHomepage = null; 

        public MainWindow()
        {
            InitializeComponent();
            msgService = new MsgService();
            downloadService = new DownloadService();
            pgUtilService = new ProgramUtilService();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ShowWaringMsg();
            ProgramRegistryCheck();
            ProgramVersionCheck();
            SetHomepageUrl();
            this.Show();

            blur = new BlurEffect();
            blur.Radius = 5;
            BottomDisign bottom = new BottomDisign();
            bottom.activeUserControlEvent += ButtonAction;
            Grid_Bottom.Children.Add(bottom);
        }

        #region Method
        private void ShowProgramRegistry()
        {
            var regComponent = new ProgramRegister();
            regComponent.ShowDialog();
        }

        private void ShowWaringMsg()
        {
            var waringComponent = new WarningModal();
            waringComponent.ShowDialog(); ;
        }

        private void ProgramVersionCheck()
        {
            var currentVersion = (string)Application.Current.Resources["ProgramVersion"];
            var convertVersion = double.NaN;
            double.TryParse(currentVersion, out convertVersion);
            var downloadVersion = downloadService.GetVersion(pgUtilService.url_ProgramVersion);

            if (double.IsNaN(convertVersion) || double.IsNaN(downloadVersion))
                msgService.Show(MsgEnum.Category.Waring, MsgEnum.CloseType.Close, "프로그램 버전을 확인할 수 없습니다.\r\n최신버전을 다시 다운 받아보세요.");
            else if (convertVersion < downloadVersion)
                msgService.Show(MsgEnum.Category.Waring, MsgEnum.CloseType.Close, "프로그램이 구버전입니다.\r\n최신버전 다운로드를 권장드립니다.");

        }

        private void ProgramRegistryCheck()
        {
            var programRegPath = @"HKEY_CURRENT_USER\SOFTWARE\CatPubg";
            if (Util.Reg.Exist(programRegPath, "SteamPath") == false && Util.Reg.Exist(programRegPath, "KakaoPath") == false)
                ShowProgramRegistry();
        }

        private void SetHomepageUrl()
        {
            nyaongHomepage = downloadService.GetHomepageUrl(pgUtilService.url_NyaongPage);
        }

        private void EnableMainFormBlur()
        {
            this.Effect = blur;
        }
        private void DisableMainFormBlur()
        {
            this.Effect = null;
        }
        private void ButtonAction(UserControl userControl)
        {
            EnableMainFormBlur();

            var component = new PopupModal(userControl);
            component.Owner = this;
            component.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            component.ShowDialog();

            DisableMainFormBlur();
        }
        #endregion

        #region Handler
        private void Button_ProgramRegistry_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ShowProgramRegistry();
            this.Show();
        }
        private void Button_Homepage_Click(object sender, RoutedEventArgs e)
        {
            if(nyaongHomepage.ToLower().Equals("null"))
            {
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "준비중입니다.");
                return;
            }

            System.Diagnostics.Process.Start(nyaongHomepage);
        }
        #endregion


    }
}
