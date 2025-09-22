using System;

namespace ca.stellarforgeinteractive.silverstream.Util
{
    public class DebugException:Exception
    {
        public DebugException(string message="Debug") : base(message)
        {
        }

        public DebugException():base(message:"debug")
        {
        }
    }
}