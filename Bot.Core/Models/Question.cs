﻿using System.Collections.Generic;

namespace Template.Core.Models
{
    public class Question
    {
        public string Text { get; set; }
        public string IsAnswered { get; set; }
        public string[] Keywords { get; set; }
    }
}