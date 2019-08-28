using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonEditor.ConfigurationModels
{
    public interface IPathSettings
    {
        string PathDialogs { get; set; } 

        string PathLocations { get; set; } 

        string PathMainMenu { get; set; } 

        string PathQAs { get; set; } 

        string PathCourses { get; set; } 

        string PathPlannedEvents { get; set; } 

        string PathChooseOptionList { get; set; } 

        string PathDialogsMUI { get; set; } 
    }
}
