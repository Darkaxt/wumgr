using System.IO;

namespace wumgr
{
    class ManualInstallArguments
    {
        public static string GetExeArguments(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);

            if (name.IndexOf("scepinstaller", System.StringComparison.CurrentCultureIgnoreCase) == 0)
                return "/s /q";

            if (name.IndexOf("ndp", System.StringComparison.CurrentCultureIgnoreCase) == 0 ||
                name.IndexOf("OFV", System.StringComparison.CurrentCultureIgnoreCase) == 0 ||
                name.IndexOf("2553065", System.StringComparison.CurrentCultureIgnoreCase) == 0)
                return "/q /norestart";

            return "/q /z";
        }
    }
}
