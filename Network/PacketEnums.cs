using System;

namespace PacketEnums
{
    public enum LoginPlatform : short
    {
        Invalid,
        Guest,
        Device,
        Betakey,
        Facebook,
        Naver,
        GooglePlay,
        GameCenter,
    }

    public enum pe_Team : int
    {
        Invalid,
        Main,
        PVP,
        Event = 1000,
        WorldBoss = 2001,
        Boss = 10001,
        Adventure = 20000,
        PVP_Defense=100000,
    }

    public enum pe_UseLeaderSkillType : int
    {
        Manual,
        Start,
        LastWave,
        SelfDanger,
        TeamDanger,
    }


    public enum pe_Battle : short
    {
        Stage,
    }

    public enum pe_EndBattle
    {
        Invalid,
        Win,
        Lose,
        Timeout,
        Exit,
    }

    public enum pe_NicknameResult
    {
        Invalid,
        Success,
        AlreadyUse,
    }

    public enum pe_FriendsResult
    {
        Success = 0,
        FriendsCountMax = 60100,
        AlreadyRequest = 60101,
        AlreadyRequested = 60102,
        InvalidRequest = 60103,
        NotAvailableGift = 60104,
        LimitGiftMax = 60105,
        LimitDeleteFriends = 60106,
        AlreadySendGift = 60107,
        TargetFriendsCountMax = 60108,
        FriendsRequestCountMax = 60109,
        NotExistsNickname = 60110,
        AlreadyFriends = 60111,
        NotExistSendGift = 60112,
    }
    public enum pe_MsgType
    {
        Normal = 0,
        RecvWhisper = 1,
        SendWhisper = 2,
        Guild = 3,
        Yell = 4,
        Notify = 5,
        System = 6,
        Emergency = 7,
        Push = 8,

        Item = 9,
    }

    public enum pe_TakeWhere
    {
        LootCreature = 0,
        LootCreature10 = 1,
        LootRune = 2,
        LootRune10 = 3,
        CreatureMix = 4,
        CreatureEvolve = 5,
        RuneUpgrade = 6,
        BossReward = 7,
    }

    public enum pe_Difficulty : int
    {
        Normal,
        Hard,
        Harder,
        Extream,
        Hell,
    }

    public enum pe_MailType : short
    {
        Event = 0,
        PvpReward = 1,
        Attend = 2,
        GMProvide = 3,
        System = 4,
        WorldBossReward = 5,
    }

    public enum pe_HubType
    {
        SmallHeroChat,
    }

    public enum pe_HubErrorCode
    {
        Normal = 0,        
        InvalidHeader = 1,
        TimeCheckOut = 2,
        //no reconnect 
        Maintenance = 100,
    }

    public enum pe_UnreadMailState : byte
    {
        None = 0,
        UnreadMail = 1,
        MainMenuOpen = 2,
    }

    public enum pe_GuildResult
    {
        Success,
        JoinAnotherGuild,
        GuildMemberFull,
        GuildJoinTimeDelay,
        GuildMasterLeaveLimit,
        GuildRequestCountMax,
        RequestCountMax,
        SameGuildName,
        LimitLevel, 
        NotExistGuild,
    }

}
