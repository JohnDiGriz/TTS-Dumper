using System;
using System.Collections.Generic;
using System.Text;

namespace TTS_Dumper
{
    public class ModModel
    {
        public string Name { get; set; }

        public Dictionary<string, IEnumerable<string>> Data { get; set; }

        public IEnumerable<string> UnknownFiles { get; set; }
    }
}
