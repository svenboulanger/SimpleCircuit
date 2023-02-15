using System;
using System.Collections.Generic;

namespace SimpleCircuitOnline.Shared
{
    public class UploadEventArgs : EventArgs
    {
        public string Filename { get; set; }
        public string Script { get; set; }
        public string Style { get; set; }
        public string Version { get; set; }
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();
    }
}
