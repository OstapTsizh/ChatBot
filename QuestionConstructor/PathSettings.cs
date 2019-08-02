using System;
using System.Collections.Generic;
using System.Text;

namespace DecisionMakers
{
    public class PathSettings
    {
        public string PathDialogs { get; set; } //= @"..\Bot.Core\Dialogs.json";

        public string PathLocations { get; set; } //= @"..\Bot.Core\DataFiles\Locations.json";

        public string PathMainMenu { get; set; } //= @"..\Bot.Core\DataFiles\MainMenu.json";

        public string PathQAs { get; set; } //= @"..\Bot.Core\DataFiles\QAs.json";

        public string PathCourses { get; set; } //= @"..\Bot.Core\DataFiles\Courses.json";

        public string PathPlannedEvents { get; set; } //= @"..\Bot.Core\DataFiles\PlannedEvents.json";

        public string PathChooseOptionList { get; set; } //= @"..\Bot.Core\DataFiles\ChooseOptionList.json";
    }
}
