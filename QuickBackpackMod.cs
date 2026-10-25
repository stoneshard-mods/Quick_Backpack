using System;
using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace Quick_Backpack;

/// <summary>
/// 原地维修、远程开关大篷车仓库、行囊内直接打开背包、非空背包放回行囊。
/// </summary>
public class QuickBackpackMod : Mod
{
    public override string Name => "Quick_Backpack";
    public override string Author => "Pixel32E-QQ:3566723459";
    public override string Description => "行囊增强：原地维修、远程开关仓库、行囊内直接打开背包、非空背包放回行囊";
    public override string ShortDesc => "Backpack: in-place repair, remote stash, open backpack in bag";
    public override string Version => "v1.6.0.0";

    public override void PatchMod()
    {
        // 行囊工具栏的两个按钮：继承 o_button_actionkey（与整理按钮同源），
        // 悬停高亮、按下、点击音效、悬浮提示等全部复用原生逻辑。
        Msl.AddObject("o_qb_stash_btn", spriteName: "s_qb_stash_button", parentName: "o_button_actionkey", isVisible: true, isPersistent: false, isAwake: true);
        Msl.AddNewEvent("o_qb_stash_btn", Code("o_qb_stash_btn_Create.gml"), EventType.Create, 0);
        Msl.AddNewEvent("o_qb_stash_btn", Code("o_qb_stash_btn_Other_10.gml"), EventType.Other, 10);

        Msl.AddObject("o_qb_repair_btn", spriteName: "s_qb_repair_button", parentName: "o_button_actionkey", isVisible: true, isPersistent: false, isAwake: true);
        Msl.AddNewEvent("o_qb_repair_btn", Code("o_qb_repair_btn_Create.gml"), EventType.Create, 0);
        Msl.AddNewEvent("o_qb_repair_btn", Code("o_qb_repair_btn_Other_10.gml"), EventType.Other, 10);

        // 行囊打开时创建两个按钮。
        // 锚点选在全局物品栏容器创建之后（cellsContainer = ...），保证工具栏容器已经存在，
        // 按钮直接挂到行囊实例下即可。
        InsertAfter(
            "gml_Object_o_inventory_Create_0",
            new[]
            {
                "cellsContainer = scr_inventory_container_create",
                "itemsContainer = scr_guiCreateContainer(id, 85,",
                "itemsContainerWidth"
            },
            Code("patch_buttons_create.gml"),
            "行囊工具栏按钮创建");

        // 远程打开的仓库在关闭时不要连带关掉行囊。
        // 原生 o_stash_inventory_left 的关闭事件会 with (o_inventory) event_user(2)。
        ReplaceBlock(
            "gml_Object_o_stash_inventory_left_Other_12",
            new[]
            {
                "with (o_inventory)\n{\ncloseLeftMenu = false\nevent_user(2)\ncloseLeftMenu = true\n}",
                "with (o_inventory)\n{\ncloseLeftMenu = false;\nevent_user(2);\ncloseLeftMenu = true;\n}"
            },
            Code("patch_stash_close.gml"),
            "远程仓库关闭");

        // 维修改为原版技能模式（o_skill_repair_item_master）。
        // 原版所有相关逻辑都依赖商人实例 o_trade_inventory，以下补上无商人时的分支。
        ReplaceBlock(
            "gml_Object_o_inv_slot_Step_0",
            new[] { "repair_cost = max(1, math_round(" },
            Code("patch_repair_cost.gml"),
            "维修费用公式");

        // 原版的 repair_cost 只在 if (_is_trade) 分支里赋值，远程维修没有商人，
        // 走的是 else 分支，repair_cost 恒为 0 —— 表现就是「维修成功但不扣钱」。
        // 在非交易分支补一份与铁匠铺同构的报价（系数 0.5）。
        ReplaceBlock(
            "gml_Object_o_inv_slot_Step_0",
            new[] { "price = math_round(base_price * _durable_price * _stack_mod)" },
            Code("patch_repair_cost_remote.gml"),
            "远程维修费用公式");

        ReplaceBlock(
            "gml_Object_o_skill_repair_item_master_Other_11",
            new[] { "scr_gold_add_npc(repair_cost)" },
            Code("patch_repair_gold.gml"),
            "维修扣费");

        ReplaceBlock(
            "gml_Object_o_skill_repair_item_master_Other_20",
            new[] { "scr_npc_check_item_stolen_location(o_trade_inventory.owner)" },
            Code("patch_repair_stolen.gml"),
            "维修高亮判定");

        // 没有商人实体，master_npc 不存在；把唯一读取它的两处改成「全部材质可修」
        ReplaceBlock(
            "gml_Object_o_skill_repair_item_master_Other_20",
            new[] { "var _material_spec = master_npc.Material_Spec" },
            Code("patch_repair_material.gml"),
            "维修材质范围");

        // 原版逐个比对材质字符串，不认 "all"；这里补上 "all" 的判定
        ReplaceBlock(
            "gml_Object_o_skill_repair_item_master_Other_20",
            new[] { "var _is_material = false" },
            Code("patch_repair_ismaterial.gml"),
            "维修材质判定");

        ReplaceBlock(
            "gml_Object_o_hoverWeapon_Other_20",
            new[] { "with (master_npc)\n_canRepair = (scr_array_get_index(Material_Spec, _material)" },
            Code("patch_repair_canrepair.gml"),
            "武器提示的可修判定");

        // 原版不允许把「装着东西的背包」放进行囊：scr_backpack_is_empty 对所有非空背包一律拒绝，
        // 于是右键「移除」被灰掉、拖到行囊格也落不下去。只对「已装备 / 刚拖起来 / 本来就在行囊里」
        // 且没打开的背包放行；从地上捡起、从容器里全部取走等原生行为保持不变。
        ReplaceBlock(
            "gml_GlobalScript_scr_can_replace_item",
            new[] { "if (object_is_ancestor(argument0.object_index, o_inv_backpack_parent)" },
            Code("patch_backpack_is_empty.gml"),
            "非空背包可放回行囊");

        // 原版右键菜单里背包本来就有一项原生「打开」（case "Use"），只是被
        // scr_can_use_items 的 `backpack && !equipped → false` 判成灰色不可点。
        // 这里只给「行囊内 + 未打开 + 非交易」的背包放行，原生那一项自己变亮。
        ReplaceBlock(
            "gml_GlobalScript_scr_can_use_items",
            new[] { "if (object_is_ancestor(object_index, o_inv_backpack_parent)" },
            Code("patch_can_use_backpack.gml"),
            "行囊内背包可用（原生打开）");
    }

    private static void ReplaceBlock(string codeName, string[] anchors, string code, string what)
    {
        InvalidOperationException lastMiss = null;

        foreach (string anchor in anchors)
        {
            try
            {
                Msl.LoadGML(codeName).MatchFrom(anchor).ReplaceBy(code).Save();
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("MatchFrom"))
            {
                lastMiss = ex;
            }
        }

        throw new InvalidOperationException(
            "Quick_Backpack 无法定位补丁位置（" + what + "）：" + codeName, lastMiss);
    }

    private static void InsertAfter(string codeName, string[] anchors, string code, string what)
    {
        InvalidOperationException lastMiss = null;

        foreach (string anchor in anchors)
        {
            try
            {
                Msl.LoadGML(codeName).MatchFrom(anchor).InsertBelow(code).Save();
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("MatchFrom"))
            {
                // 只是没找到锚点，换下一个候选；GML 编译错误会以别的异常直接抛出。
                lastMiss = ex;
            }
        }

        throw new InvalidOperationException(
            "Quick_Backpack 无法定位补丁位置（" + what + "）：" + codeName, lastMiss);
    }

    private string Code(string fileName) => ModFiles.GetCode(fileName);
}
