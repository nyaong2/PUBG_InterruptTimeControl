﻿using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

namespace PUBG_InterruptTimeControl.Components.Function.Etc
{
    public partial class WindowsVarietyInstruction : UserControl
    {
        private MsgService msgService;
        public WindowsVarietyInstruction()
        {
            InitializeComponent();
            msgService = new MsgService();
        }

        private void WindowsVarietyInstruection_Loaded(object sender, RoutedEventArgs e)
        {
            LabelChangeCurrentMemoryCompression();
        }


        #region Function
        private bool LabelChangeCurrentMemoryCompression()
        {
            var result = Util.Dos.Ps("(Get-MMAgent).MemoryCompression");

            if (result.ToLower().Contains("false")) //꺼진게 적용된 것임.
            {
                Label_CurrentMemoryCompression.Content = "비활성화";
                return true;
            }
            else
            {
                Label_CurrentMemoryCompression.Content = "활성화";
                return false;
            }
        }
        private void MemoryCompressionApply()
        {
            var result = Util.Dos.Ps("Disable-MMAgent -MemoryCompression");

            if (LabelChangeCurrentMemoryCompression())
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "메모리 압축 기능이 해제되었습니다.");
            else
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용에 실패했습니다.");

        }
        private void MemoryCompressionRestore()
        {
            var result = Util.Dos.Ps("Enable-MMAgent -MemoryCompression");

            LabelChangeCurrentMemoryCompression();

            if (LabelChangeCurrentMemoryCompression())
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "메모리 압축 기능이 원상복구 되었습니다.");
            else
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "원상복구에 실패했습니다.");
        }
        #endregion

        #region Handler
        private void Button_MemoryCompressionApply_Click(object sender, RoutedEventArgs e)
        {
            MemoryCompressionApply();
        }
        private void Button_MemoryCompressionRestore_Click(object sender, RoutedEventArgs e)
        {
            MemoryCompressionRestore();
        }
        #endregion

    }
}
