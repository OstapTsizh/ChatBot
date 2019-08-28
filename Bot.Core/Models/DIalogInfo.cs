namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Contains info to make possible dialog logging.
    /// </summary>
    public class DialogInfo
    {
        public string UserId { get; set; }

        public int DialogId { get; set; }

        public string LastDialogName { get; set; }
        
        /// <summary>
        /// Specified country name.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Specified city in country.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Language to display messages/info.
        /// </summary>
        public string Language { get; set; }
    }
}
