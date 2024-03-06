using PUBG_InterruptTimeControl.Components.Modal.Action;
using System;
using System.Collections.Generic;
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

namespace PUBG_InterruptTimeControl.Components.Modal
{
    /// <summary>
    /// CustomModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MsgModal : Window
    {
        public string content { get; set; }
        public string ImagePath { get; set; }
        public int reference_Result { get; set; }

        public MsgModal()
        {
            InitializeComponent();
            Image_enum.DataContext = this;
        }

        #region Function
        public void Message(MsgEnum.Category category, MsgEnum.CloseType closeType, string content)
        {
            SetCategory(category);
            SetCloseType(closeType);
            Label_Msg.Content = content;
        }

        private void SetCategory(MsgEnum.Category category)
        {
            switch(category)
            {
                case MsgEnum.Category.Info:
                    ImagePath = "/Resources/Image/Msg/information.png";
                    break;
                case MsgEnum.Category.Waring:
                    ImagePath = "/Resources/Image/Msg/warning.png";
                    break;
                case MsgEnum.Category.Error:
                    ImagePath = "/Resources/Image/Msg/error.png";
                    break;
            }
        }
        private void SetCloseType(MsgEnum.CloseType closeType)
        {
            switch(closeType)
            {
                case MsgEnum.CloseType.Close:
                    Grid_Close.Visibility = Visibility.Visible;
                    break;
                case MsgEnum.CloseType.YesNo:
                    Grid_YesNo.Visibility = Visibility.Visible;
                    break;
            }
        }
        #endregion

        #region Handler
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            reference_Result = (int)MsgEnum.Result.Close;
            this.Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            reference_Result = (int)MsgEnum.Result.Yes;
            this.Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            reference_Result = (int)MsgEnum.Result.No;
            this.Close();
        }
        #endregion
    }
}
