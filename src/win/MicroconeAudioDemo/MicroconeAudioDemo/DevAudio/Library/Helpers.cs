using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroconeSDKDemo.DevAudio.Library
{
    public static class Helpers
    {
        public static String GetDocumentsDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }
}
