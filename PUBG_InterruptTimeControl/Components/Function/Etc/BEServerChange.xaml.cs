using PUBG_InterruptTimeControl.Components.Modal;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.PgReg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace PUBG_InterruptTimeControl.Components.Function.Etc
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BEServerChange : UserControl
    {
        MsgService msgService;
        ProgramUtilService pgRegService;
        private const string reg_BePath = @"HKEY_CURRENT_USER\System\GameConfigStore\Children";
        private string query = null;
        string reg_FindBePath;
        
        public BEServerChange()
        {
            InitializeComponent();
            msgService = new MsgService();
            pgRegService = new ProgramUtilService();

            query = "REG QUERY \"HKCU\\System\\GameConfigStore\\Children\" /f \"TslGame_BE.exe\" /s 2>nul | find /i \"HKEY_CURRENT_USER\"";

        }

        private void BEServerChange_Loaded(object sender, RoutedEventArgs e)
        {
            //스배&카배 중복설치 및 한개만 설치가 됐는지 체크 & 실질적으로 필요한 tslgame_be 레지 찾기
            if ((ProperPUBGInstallCheck() && GetBePath()) == false)
            {
                ControlExit();
                return;
            }
            
            SetLabelCurrentRegApplyPath(); //tslgame_be 레지 찾고 현재 설정되어있는 위치에 따라 label 변경

        }


        #region Function
        private bool ProperPUBGInstallCheck()
        {
            var result = true;
            if (String.IsNullOrEmpty(pgRegService.reg_SteamPath) || String.IsNullOrEmpty(pgRegService.reg_KakaoPath)) // 둘 중 한곳이라도 등록이 안되어있는 경우 (설치가 둘 중 한곳이라도 안되어 있는 경우)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "설치한 서버가 한 곳인 경우 이용할 수 없습니다.");
                return false;
            }
            if (pgRegService.reg_SteamPath.Equals(pgRegService.reg_KakaoPath)) //동일한 폴더로 쓴 경우
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "각 서버간 동일한 폴더는 이용할 수 없습니다.");
                return false;
            }

            return result;
        }

        private bool GetBePath()
        {
            reg_FindBePath = Util.Dos.Cmd(query);

            if (String.IsNullOrEmpty(reg_FindBePath))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "필요한 정보를 찾을 수 없습니다.");
                return false;
            }
            else
                return true;

        }
        private void SetLabelCurrentRegApplyPath()
        {
            var data = Util.Reg.Read(reg_FindBePath, "MatchedExeFullPath");
            if (Path.GetDirectoryName(data).Equals(pgRegService.reg_SteamPath))
                Label_CurrentValue.Content = "스배";
            else if (Path.GetDirectoryName(data).Equals(pgRegService.reg_KakaoPath))
                Label_CurrentValue.Content = "카배";
        }

        private void ControlExit()
        {
            var window = Window.GetWindow(this);
            window.Close();
            return;
        }
        #endregion

        #region Handler
        private void Button_SteamApply_Click(object sender, RoutedEventArgs e)
        {
            if (Util.Reg.Write(reg_FindBePath, "MatchedExeFullPath", pgRegService.reg_SteamPath + @"\TslGame_BE.exe", Util.Reg.RegValueKind.SZ))
            {
                Label_CurrentValue.Content = "스배";
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "스팀배그 위치로 적용이 완료되었습니다.");
            }
            else
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
        }

        private void Button_KakaoApply_Click(object sender, RoutedEventArgs e)
        {
            if (Util.Reg.Write(reg_FindBePath, "MatchedExeFullPath", pgRegService.reg_KakaoPath + @"\TslGame_BE.exe", Util.Reg.RegValueKind.SZ))
            {
                Label_CurrentValue.Content = "카배";
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "다음배그 위치로 적용이 완료되었습니다.");
            }
            else
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");
        }
        #endregion

    }
}
