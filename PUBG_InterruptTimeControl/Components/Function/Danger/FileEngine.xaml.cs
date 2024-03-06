using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PUBG_InterruptTimeControl.Components.Function.Danger
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileEngine : UserControl
    {
        private readonly MsgService msgService;
        private readonly string engineFilePath = Environment.GetEnvironmentVariable("LOCALAPPDATA") + @"\TslGame\Saved\Config\WindowsNoEditor\Engine.ini";



        public FileEngine()
        {
            InitializeComponent();
            msgService = new MsgService();
            CurrentEngineResolution();
        }

        #region Function
        private void CurrentEngineResolution()
        {
            try
            {
                var readEngineFile = File.ReadAllLines(engineFilePath);
                var fileAttributes = File.GetAttributes(engineFilePath);

                //해상도부분 찾고 수정
                for (int i = 0; i < readEngineFile.Length; i++)
                {
                    if (readEngineFile[i].Contains("r.setres="))
                        Label_CurrentResolution.Content = readEngineFile[i].Split('=')[1];
                }
            }
            catch { };
        }
        #endregion

        #region Handler
        private void Button_InputApply_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(TextBox_Input.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "값이 비어있습니다.");
                return;
            }
            try
            {
                var readEngineFile = File.ReadAllLines(engineFilePath);
                var fileAttribute = File.GetAttributes(engineFilePath);

                //이미 적용이 되어있는 경우 해제
                if (fileAttribute.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(engineFilePath, FileAttributes.Normal);

                //해상도부분 찾고 수정
                for (int i = 0; i < readEngineFile.Length; i++)
                {
                    if (readEngineFile[i].Contains("r.setres="))
                    {
                        readEngineFile[i] = "";
                        readEngineFile[i] = "r.setres=" + TextBox_Input;
                        break;
                    }
                }

                //수정된 내용 쓰기 및 읽기전용 적용
                File.WriteAllLines(engineFilePath, readEngineFile, Encoding.UTF8);
                File.SetAttributes(engineFilePath, FileAttributes.ReadOnly);

                // 현재 값 label 설정 및 텍스트박스 비우기
                Label_CurrentResolution.Content = TextBox_Input.Text;
                TextBox_Input.Text = "";
            } catch { };
        }

        private void Button_Restore_Click(object sender, RoutedEventArgs e)
        {
            var restoreResolution = "1280x720f";
            try
            {
                var readEngineFile = File.ReadAllLines(engineFilePath);
                var fileAttribute = File.GetAttributes(engineFilePath);

                //이미 적용이 되어있는 경우 해제
                if (fileAttribute.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(engineFilePath, FileAttributes.Normal);

                //해상도부분 찾고 수정
                for (int i = 0; i < readEngineFile.Length; i++)
                {
                    if (readEngineFile[i].Contains("r.setres="))
                    {
                        readEngineFile[i] = "";
                        readEngineFile[i] = "r.setres=" + restoreResolution;
                        break;
                    }
                }

                //수정된 내용 쓰기 및 읽기전용 적용
                File.WriteAllLines(engineFilePath, readEngineFile, Encoding.UTF8);

                // 현재 값 label 설정 및 텍스트박스 비우기
                Label_CurrentResolution.Content = restoreResolution;
                TextBox_Input.Text = "";
            }
            catch { };
        }

        private void FileEngine_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_Input.Focus();
        }
        #endregion


    }
}
