using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroconeSDKDemo.DevAudio.Microcone
{
    public class Metadata
    {
        public enum RecordButtonModes { Record, Pause };
        public enum RecordingMode { Stopped, Paused, Recording, Busy };
        public static string ImagesBasePath
        {
            get
            {
                return @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name
                                + ";component/Assets/Images/";
            }
        }

        public const string csMixFileFormat = "mix-{0}.{1}";
        public const string csChannelFileFormat = "sector-{0}.{1}";
        public const string csSaveFileExtension = "wav";

    }
}
