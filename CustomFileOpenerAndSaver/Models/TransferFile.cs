using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Models
{
    public class TransferFile
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("extension")]
        public string Extension { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; }
    }
}
