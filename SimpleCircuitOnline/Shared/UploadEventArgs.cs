using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public abstract class UploadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the filename.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets the diagnostic messages.
        /// </summary>
        public List<IDiagnosticMessage> Messages { get; } = [];
    }
    public class UploadLibraryEventArgs : UploadEventArgs
    {
        
        public XmlDocument Document { get; set; }
    }
    public class UploadSvgEventArgs : UploadEventArgs
    {
        public string Script { get; set; }
        public string Version { get; set; }
    }
}
