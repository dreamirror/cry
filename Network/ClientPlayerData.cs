using PacketInfo;
using System;
using System.Collections.Generic;

public class ClientPlayerData : pd_PlayerData
{
    public ClientPlayerData(pd_PlayerData data)
    {
        foreach (System.Reflection.FieldInfo field_info in typeof(pd_PlayerData).GetFields())
        {
            field_info.SetValue(this, field_info.GetValue(data));
        }
        if (goods == null) goods = new List<pd_GoodsData>();
    }

    public void SetPlayerData(pd_PlayerData data)
    {
        foreach (System.Reflection.FieldInfo field_info in typeof(pd_PlayerData).GetFields())
        {
            field_info.SetValue(this, field_info.GetValue(data));
        }
    }

    public long GetGoodsValue(pe_GoodsType goods_type)
    {
        if (goods == null)
            return 0;

        var goods_info = goods.Find(g => g.goods_type == goods_type);
        if (goods_info == null)
            return 0;

        return goods_info.goods_value;
    }

    public void SetGoodsValue(pe_GoodsType goods_type, long goods_value)
    {
        var goods_info = goods.Find(g => g.goods_type == goods_type);
        if (goods_info == null)
            goods.Add(new pd_GoodsData(goods_type, goods_value));
        else
            goods_info.goods_value = goods_value;
    }
    public void AddGoods(pd_GoodsData goods)
    {
        AddGoodsValue(goods.goods_type, goods.goods_value);
    }
    public bool UseGoods(pd_GoodsData goods)
    {
        return UseGoodsValue(goods.goods_type, goods.goods_value);
    }

    public void AddGoodsValue(pe_GoodsType goods_type, long goods_value)
    {
        if (goods == null) return;

        if (goods_type == pe_GoodsType.token_energy)
        {
            Network.PlayerInfo.AddEnergy((int)goods_value);
            return;
        }

        var goods_info = goods.Find(g => g.goods_type == goods_type);
        if (goods_info == null)
        {
            goods.Add(new pd_GoodsData(goods_type, goods_value));
            return;
        }

        goods_info.goods_value += goods_value;
    }

    public bool UseGoodsValue(pe_GoodsType goods_type, long goods_value)
    {
        if (goods == null) return false;

        var goods_info = goods.Find(g => g.goods_type == goods_type);
        if (goods_info == null) return false;

        if (goods_info.goods_value < goods_value) return false;
        goods_info.goods_value -= goods_value;
        return true;
    }

    public EventParamLevelUp UpdateExp(pd_PlayerExpAddInfo exp_add_info)
    {
        EventParamLevelUp param = new EventParamLevelUp();
        param.old_level = player_level;
        param.old_exp = player_exp;
        param.old_energy = GetEnergy();
        if (exp_add_info == null)
        {
            param.add_exp = 0;
            param.new_level = player_level;
            param.new_exp = player_exp;
            param.new_energy = param.old_energy;
            return param;
        }

        param.add_exp = exp_add_info.add_player_exp;
        param.new_level = exp_add_info.player_level;
        param.new_exp = exp_add_info.player_exp;
        param.new_energy = (short)(param.old_energy + exp_add_info.energy_bonus);

        player_level = exp_add_info.player_level;
        player_exp = exp_add_info.player_exp;
        energy_max = LevelInfoManager.Instance.GetEnergyMax(param.new_level);
        if(param.old_energy < GetEnergy())
            exp_add_info.energy_bonus -= (short)(GetEnergy() - param.old_energy);

        if (exp_add_info.energy_bonus > 0)
            Network.PlayerInfo.AddEnergy(exp_add_info.energy_bonus);

        return param;
    }

    public short GetEnergy()
    {
        short energy_regen_time = GameConfig.Get<short>("energy_regen_time");
        return (short)(Math.Min(energy_max, ((Network.Instance.ServerTime - energy_time).TotalSeconds) / energy_regen_time) + additional_energy);
    }

    public void UseEnergy(short use_energy)
    {
        if (additional_energy >= use_energy)
        {
            additional_energy -= use_energy;
            return;
        }
        else
        {
            use_energy -= additional_energy;
            additional_energy = 0;
        }

        int energy_regen_time = GameConfig.Get<int>("energy_regen_time");
        int cur_energy = GetEnergy();

        if (cur_energy == energy_max)
            energy_time = Network.Instance.ServerTime.AddSeconds(-(cur_energy - use_energy) * energy_regen_time);
        else
            energy_time = energy_time.AddSeconds(use_energy * energy_regen_time);

        if (ConfigData.Instance.UseEnergyFullChargeAlarm)
            PushManager.Instance.RefreshEnergy();
    }

    public void AddEnergy(int add_energy)
    {
        int energy_regen_time = GameConfig.Get<int>("energy_regen_time");
        int energy = GetEnergy() + add_energy;
        if(energy >= energy_max)
        {
            additional_energy = (short)(energy - energy_max);
        }
        energy_time = energy_time.AddSeconds(-add_energy * energy_regen_time);
        
        if (ConfigData.Instance.UseEnergyFullChargeAlarm)
            PushManager.Instance.RefreshEnergy();
    }

    public int GetSlotBuyCash(SlotInfo slot)
    {
        int price = slot.CashDefault + slot.CashAdd * creature_count_buy_count;
        int price_max = slot.CashMax;
        if (price_max > 0)
            price = Math.Min(price, price_max);
        return price;
    }
}
