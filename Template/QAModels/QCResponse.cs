using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Template.QAModels
{
    public class QCResponse
    {
        public string Question { get; set; }

        public bool HasNextQuestion { get; set; }

        public List<string> Options { get; set; }
    }
}
