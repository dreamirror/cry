using UnityEngine;

public class PopupCommunity : PopupMainMenu
{
    override protected void InitItem()
    {
        //[친구 규칙 설명]\n
        string friend_text = "1. 다른 유저를 친구로 등록할 수 있습니다.\n2. 친구의 대표영웅을 용병으로 사용할 수 있습니다.\n3. 친구의 토벌전에 참여할 수 있습니다.";

        var friend = m_PrefabManager.GetNewObject<PopupMenuItem>(m_Grid.transform, Vector3.zero);
        string title = Localization.Get("Menu_Friends");
        string desc = "";
        friend.Init("friend", title, desc, "main_menu_community_friend", friend_text, OnClick);

        //[길드 규칙 설명]\n
        string guild_text = "1. 길드에 가입하면 길드전에 참여할 수 있습니다.\n2. 내 영웅을 길드 용병으로 등록할 수 있습니다.\n3. 길드 관리자는 등록된 용병으로 길드군단을 편성할 수 있습니다.\n4. 길드에 등록된 모든 영웅으로 하나의 군단을 만듭니다.\n5. 총 5개의 파티 슬롯에 영웅을 순서대로 배치합니다.\n6. 길드 관리자는 편성한 군단으로 길드전에 참여할 수 있습니다.";

        var guild = m_PrefabManager.GetNewObject<PopupMenuItem>(m_Grid.transform, Vector3.zero);
        title = Localization.Get("Menu_Guild");
        desc = "내 길드 : 몬스터스마일";
        guild.Init("guild", title, desc, "main_menu_community_guild", guild_text, OnClick);
    }

    void OnClick(string menu_id)
    {
        switch (menu_id)
        {
            case "friend":
                GameMain.Instance.ChangeMenu(GameMenu.Friends);
                OnCancel();
                break;
            case "guild":
                Tooltip.Instance.ShowMessageKey("NotImplement");
                break;

        }
    }
}
