using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonEditor.ConfigurationModels
{
    public class PathSettingsLocal
    {
        public string IsLocalStart { get; set; }

        public string RootForLocalStart { get; set; }

        public string RootForRemoteStart { get; set; }
    }
}
