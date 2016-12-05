using PacketEnums;
using System.Collections.Generic;

namespace C2D
{
    public class CrashReport
    {
        public long file_account_idx;
        public string report;
        public string stack_trace;
        public List<string> log;
        public C2L.AppInfo app_info;
    }

}