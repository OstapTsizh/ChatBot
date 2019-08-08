using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Represents country where EPAM gives training.
    /// </summary>
    public class Country
    {
        /// <summary>
        /// A name of specified country.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// A list of cities in the specified country
        /// where EPAM gives training.
        /// </summary>
        public List<string> Cities { get; set; }
    }
}
