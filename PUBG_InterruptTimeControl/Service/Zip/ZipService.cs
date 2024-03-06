using PUBG_InterruptTimeControl.Components.Modal;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Download;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Msg;
using PUBG_InterruptTimeControl.Service.Wrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PUBG_InterruptTimeControl.Service.Zip
{
    internal class ZipService
    {
        private readonly MsgService msgService;
        private static ZipService instance;

        public ZipService() 
        {
            msgService = new MsgService();
        }

        public static ZipService Instance
        {
            get
            {
                if (Instance == null)
                    instance = new ZipService();
                return instance;
            }
        }

        public ResultEnum Extract(string path, string extractPath)
        {
            var modal = new ZipModal(path,extractPath);
            modal.ShowDialog();
            var result = modal.result.Value;

            if (result == ResultEnum.Error)
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "압축 풀기에 실패했습니다.");

            try
            {
                File.Delete(path);
            }
            catch { };
            return result;
        }
    }
}
