using PUBG_InterruptTimeControl.Components.Modal;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PUBG_InterruptTimeControl.Service.Msg
{
    internal class MsgService
    {
        private static MsgService instance;
        //private MsgModal msgModal;

        public MsgService() { }

        public static MsgService Instance
        {
            get
            {
                if (instance == null)
                    instance = new MsgService();
                return instance;
            }
        }

        public int Show(MsgEnum.Category category, MsgEnum.CloseType closeType, string content)
        {
            var msgModal = new MsgModal();
            msgModal.Message(category, closeType, content);
            msgModal.ShowDialog();
            return msgModal.reference_Result;
        }
    }
}
