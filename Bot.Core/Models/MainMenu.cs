using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Represents main menu (e.g. navigation bar)
    /// </summary>
    public class MainMenu
    {
        /// <summary>
        /// Items of main menu (localized)
        /// </summary>
        public List<MainMenuItem> Items { get; set; }

        /// <summary>
        /// Items of main menu
        /// </summary>
        public List<string> Items_neutral { get; set; }
    }
}
