using System;

namespace SimpleCircuitOnline.Shared
{
    public class UploadEventArgs : EventArgs
    {
        public string Script { get; set; }
        public string Style { get; set; }
        public string Errors { get; set; }
        public string Warnings { get; set; }
    }
}
