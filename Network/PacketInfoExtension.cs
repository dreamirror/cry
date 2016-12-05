namespace PacketInfo
{
    public static class PacketInfoExtension
    {
        public static short GetDailyClearCount(this pd_MapClearData data)
        {
            if (data.daily_index != Network.DailyIndex)
                return 0;
            return data.daily_clear_count;
        }
    }
}