using System.IO;
using System.Text;

namespace SimpleCircuitOnline
{
    /// <summary>
    /// A string writer that forces UTF8 encoding.
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        /// <inheritdoc />
        public override Encoding Encoding => Encoding.UTF8;
    }
}
