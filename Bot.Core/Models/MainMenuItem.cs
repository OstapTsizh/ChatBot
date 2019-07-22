using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Represents a main menu item.
    /// </summary>
    public class MainMenuItem
    {
        /// <summary>
        /// Name of main menu item.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Path to the related json file where are
        /// nested menu items. Can be empty.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// A list of resources (some information).
        /// </summary>
        public List<string> Resources { get; set; }

        /// <summary>
        /// A name of the needed dialog.
        /// </summary>
        public string Dialog { get; set; }
    }
}
