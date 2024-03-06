using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PUBG_InterruptTimeControl.Service.PgReg
{
    internal class ProgramUtilService
    {
        private static ProgramUtilService instance;

        public readonly string url_NyaongPage = "https://raw.githubusercontent.com/nyaong2/CatProgramData/main/PUBG_InterruptTimeControl/Homepage.txt";

        public readonly string url_ProgramVersion = "https://raw.githubusercontent.com/nyaong2/CatProgramData/main/PUBG_InterruptTimeControl/Data/ProgramVersion.txt";
        public readonly string url_DatVersion = "https://raw.githubusercontent.com/nyaong2/CatProgramData/main/PUBG_InterruptTimeControl/Data/dat/datVersion.txt";
        public readonly string url_DatFile = "https://github.com/nyaong2/CatProgramData/raw/main/PUBG_InterruptTimeControl/Data/dat/dat.zip";
        public readonly string url_BinVersion = "https://raw.githubusercontent.com/nyaong2/CatProgramData/main/PUBG_InterruptTimeControl/Data/bin/binVersion.txt";
        public readonly string url_BinFile = "https://github.com/nyaong2/CatProgramData/raw/main/PUBG_InterruptTimeControl/Data/bin/bin.zip";
        public readonly string url_EtcVersion = "https://raw.githubusercontent.com/nyaong2/CatProgramData/main/PUBG_InterruptTimeControl/Data/etc/etcVersion.txt";
        public readonly string url_EtcFile = "https://github.com/nyaong2/CatProgramData/raw/main/PUBG_InterruptTimeControl/Data/etc/etc.zip";

        public readonly string reg_Path = @"HKEY_CURRENT_USER\SOFTWARE\CatPubg";
        public readonly string reg_SteamPath, reg_KakaoPath;
        public readonly string ProgramVersion = Application.Current.Resources["ProgramVersion"] as string;
        public readonly string processesFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        public static ProgramUtilService Instance
        {
            get
            {
                if (instance == null)
                    instance = new ProgramUtilService();
                return instance;
            }
        }

        public ProgramUtilService()
        {
            reg_SteamPath = Util.Reg.Read(reg_Path, "SteamPath");
            reg_KakaoPath = Util.Reg.Read(reg_Path, "KakaoPath");
        }

        public bool Registerer(string regPath, string regName, string data, Util.Reg.RegValueKind regType)
        {
            return Util.Reg.Write(regPath, regName, data, regType);
        }
        public bool RegDelete(string regPath, string regName)
        {
            return Util.Reg.Delete(regPath, regName);
        }

        public double GetFileVersion(string filePath)
        {
            double version = double.NaN;
            if (File.Exists(filePath))
                return version;

            try
            {
                var data = File.ReadAllText(filePath);
                double.TryParse(data, out version);
            }
            catch { };

            return version;
        }

        public void SetFileVersion(string filePath, double version)
        {
            File.WriteAllText(filePath, version.ToString());
        }

    }
}
