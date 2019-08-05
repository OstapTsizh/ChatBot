using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Represents a planned event in EPAM.
    /// </summary>
    public class PlannedEvent
    {
        /// <summary>
        /// Name of a planned event.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A list of resources (some information).
        /// </summary>
        public List<string> Resources { get; set; }
    }
}
