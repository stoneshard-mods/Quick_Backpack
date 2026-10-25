if (instance_exists(o_trade_inventory))
{
    scr_gold_add_npc(repair_cost);
}
else
{
    // arg1 传 undefined：这是原版在「无商人」场景下的标准调用方式
    // （villagePanic / branch_study / caravanFollowerMasterMarch 都是这么用的），
    // 内部会退化成 arg1 = instance_find(o_trade_inventory, 0) = -4，
    // 排除表只剩 -4，因此玩家身上所有金币（含钱袋、Brynn 兑换单）都会被正常扣除。
    // 不要传 -4：显式 -4 会走「排除右侧玩家容器」的分支，导致行囊里的金币扣不掉。
    scr_gold_write_off(repair_cost, undefined, true);
}
