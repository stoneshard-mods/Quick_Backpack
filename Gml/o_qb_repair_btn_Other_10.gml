// 点击：进入原版维修模式（与铁匠铺 o_repair_button 同一套逻辑）
// 区别只是不传 master_npc —— 原版仅在两处读取它（维修高亮与武器提示），模组已把这两处补丁成「全部材质可修」
if (!instance_exists(o_skill_repair_item_master))
{
    if (instance_exists(o_inventory_categories_menu))
    {
        with (o_inventory_categories_menu)
        {
            categoryEvent = 1;
            event_user(0);
        }
    }
    else
    {
        with (o_inventory)
        {
            if (!active)
                event_user(1);
        }
    }

    with (instance_create_depth(global.guiMouseX, global.guiMouseY, 0, o_skill_repair_item_master))
        event_user(10);
}
