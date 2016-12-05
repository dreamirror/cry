namespace MNS
{
    public class Log
    {
        static public void Write(string strLog)
        {
            if (_Log == null)
                return;

            _Log(strLog);
        }

        static public void Write(string strLog, params object[] args)
        {
            if (_Log == null)
                return;

            _Log(string.Format(strLog, args));
        }

        static public void WriteWarning(string strLog)
        {
            if (_LogWarning != null)
                _LogWarning(strLog);
            else if (_Log != null)
                _Log("Warning : " + strLog);
        }

        static public void WriteWarning(string strLog, params object[] args)
        {
            if (_LogWarning != null)
                _LogWarning(string.Format(strLog, args));
            else if (_Log != null)
                _Log("Warning : " + string.Format(strLog, args));
        }

        static public void WriteError(string strLog)
        {
            if (_LogError != null)
                _LogError(strLog);
            else if (_Log != null)
                _Log("Error : " + strLog);
        }

        static public void WriteError(string strLog, params object[] args)
        {
            if (_LogError != null)
                _LogError(string.Format(strLog, args));
            else if (_Log != null)
                _Log("Error : " + string.Format(strLog, args));
        }

        public delegate void _LogDelegate(string strLog);
        static public _LogDelegate _Log = null, _LogError = null, _LogWarning;
    }
}