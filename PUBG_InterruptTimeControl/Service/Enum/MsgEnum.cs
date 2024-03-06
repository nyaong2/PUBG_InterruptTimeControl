using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBG_InterruptTimeControl.Components.Modal.Action
{
    public class MsgEnum
    {
        public enum Category
        {
            Info,
            Waring,
            Error
        }

        public enum CloseType
        {
            Close,
            YesNo
        }

        public enum Result
        {
            Close,
            Yes,
            No
        }
    }
}
