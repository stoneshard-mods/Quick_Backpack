# Quick_Backpack

Stoneshard 行囊增强模组：原地维修、远程开关大篷车仓库、行囊内直接打开背包、装着东西的背包放回行囊。

- 作者：Pixel32E
- 版本：v1.6.0.0
- 依赖：ModShardLauncher + 游戏原版 `data.win`

---

## 功能

### 1. 维修按钮

行囊整理按钮右侧新增维修按钮（贴图 `s_qb_repair_button`，模组自建精灵）。点击后**进入游戏原生的维修模式**，与铁匠铺完全一致：

- 创建 `o_skill_repair_item_master`，行囊内可维修的物品高亮、不可修的变暗，光标切换为修理图标
- 鼠标悬停在物品上显示该件的维修费用，点击即修（金币不足则不生效）
- 退出方式与原版相同：右键 / Esc
- 费用公式、满耐久恢复、材质音效、`scr_characterGoldSpend("spentRepair", …)` 全部沿用游戏实现

与铁匠铺的差别只在「没有商人实例」这一点上，按有无商人分流：

| 项 | 铁匠铺（有 `o_trade_inventory`） | 远程维修（无商人） |
|---|---|---|
| 材质范围 | 商人自己的 `master_npc.Material_Spec` | `["all"]` 全部可修 |
| 报价系数 | `o_trade_inventory.owner.Repair_Prices` | **0.5** |
| 报价计算位置 | `o_inv_slot_Step_0` 的 `if (_is_trade)` 分支 | 同一事件的 `else` 分支（原版这里不算 `repair_cost`） |
| 扣费 | `scr_gold_add_npc(cost)` | `scr_gold_write_off(cost, undefined, true)` |

> `scr_gold_write_off` 的第二个参数**必须传 `undefined`**，不要传 `-4`：显式 `-4` 会走「排除右侧玩家容器」的分支，行囊里的金币扣不掉（扣费静默失败）。传 `undefined` 时内部退化成 `instance_find(o_trade_inventory, 0) == -4`、排除表只剩 `-4`，与 `scr_villagePanic` / `scr_skill_branch_study` 等无商人场景的用法一致。

原版的高亮逻辑是逐项比对材质字符串、**不认 `"all"`**，因此额外补了一处判定，否则远程模式下所有物品都会被判为不可修。

### 2. 远程仓库按钮

维修按钮右侧的按钮（贴图 `s_qb_stash_button`，模组自建精灵）可远程开关大篷车仓库。

- 与 `o_caravan_stash` 走同一套代码：`scr_guiCreateContainer(global.guiBaseContainerSideLeft, o_stash_inventory_left)` 后 `event_user(0)`
- 仓库数据在 `global.caravanStashDataList1..4`，**与玩家位置无关**，任何地方都能开
- 再点一次关闭；打开时会给该实例打 `qb_remote` 标记，关闭时**不会连带关闭行囊**（原生行为是两者一起收）

### 3. 行囊内直接打开背包

行囊里的**背包**右键菜单里的「打开」不再灰，不用先装备到背上就能开。

注意：这一项**原版本来就有**（`scr_create_context_menu` 的 `case "Use"`，标签取物品自己的 `use_text`，容器类在 `c_inv_moneybag_Other_10` 里就是 `global.context_menu[1]`），只是被下面这行判成灰色：

```gml
// scr_can_use_items
if (object_is_ancestor(object_index, o_inv_backpack_parent) && !equipped)
{
    return false;      // 未装备的背包 → 菜单项置灰
}
```

所以模组**不往菜单里加第二条**，只把这行改成放行「行囊内 + 未打开 + 非交易」的背包：

```gml
if (object_is_ancestor(object_index, o_inv_backpack_parent) && !equipped && !(instance_exists(owner) && owner.object_index == o_inventory && !is_open && !instance_exists(o_trade_inventory)))
```

原生那一项随即变亮，点击走原生 `case "Use"` → `event_user(14)` → `c_inv_moneybag_Other_24` → `scr_loadContainerContent(...)`，与「装备在背上时打开背包」完全同一条路径，菜单里也只会有一条「打开」。

- 只有背包（`o_inv_backpack_parent` 一系共 8 种：背篓 / 小背包 / 战地医药箱 / 采药挎包 / 露营背包 / 猎包 / 开门包 / 寻宝背包）走这条放行
- 腰包、钱袋、箭袋、弹药袋、箱子等不受影响，保持原生行为
- 关闭容器时原生的 `o_container_parent_Destroy_0` 会把物品 `is_open` 复位，可以反复开关
- 容器本身也是 `o_container`，算左栏容器，所以关它不会连带把行囊关掉

### 4. 装着东西的背包可以放回行囊

原版 `scr_backpack_is_empty()` 对所有「非空背包」一律返回 false，于是：背包只要里面有东西，就不能放进行囊——背着的时候右键「移除」是灰的；已经在行囊里的也不能拖到别的格子（`o_inv_slot_Other_21` 用它算落点是否有效）。

模组在 `scr_backpack_is_empty` 的判定前加了一个放行条件，目标必须是玩家行囊，且背包**没打开**、满足下面任一：

| 情形 | 判定 |
|---|---|
| 正装备在背上（卸下来） | `equipped` |
| 刚被拖起来（拖动时游戏会把 `equipped` 清掉） | `was_equipped` |
| 已经在行囊里（只是挪格子） | `owner.object_index == o_inventory` |

```gml
if ((!((argument1 == o_inventory.id && (!argument0.is_open) && (argument0.equipped || argument0.was_equipped || (instance_exists(argument0.owner) && argument0.owner.object_index == o_inventory))))) && <原判定>)
    return false;
```

从地上捡起、从容器里「全部取走」时 `owner` 不是行囊，所以仍按原生逻辑（自动装到背上）；往背包容器里塞背包也仍然是原生禁止的。

### 5. 按钮贴图（独立精灵）

两个按钮用的是模组**自己新增**的精灵，不碰游戏原版精灵：

| 文件 | 精灵 | 尺寸 | 用途 |
|---|---|---|---|
| `s_qb_repair_button_0/1/2.png` | `s_qb_repair_button` | 43×14 | 维修按钮（显示时纵向压 `13/14`） |
| `s_qb_stash_button_0/1/2.png` | `s_qb_stash_button` | 43×20 | 远程仓库按钮（显示时按 `28/43` × `13/20` 缩放） |

打包器的规则：`Textures/*.png` 按文件名（去掉 `_<帧号>`）匹配精灵——**匹配到就替换帧内容，匹配不到就新建精灵**。所以只要文件名带 `s_qb_` 前缀，就是全新精灵，`s_repair_button` / `s_stash_button`（分别被原生 `o_repair_button`、`o_stash_button` 使用）保持原样。

仓库按钮的美术稿沿用原 `s_stash_button` 的画布尺寸（43×20，含 2px 边框），工具栏行高 13 按 `13/20` 缩放显示；维修按钮是 43×14 的美术稿，按 `13/14` 压 1px。调色板取自原生按钮：`#B1B0BD`（亮）/ `#736C96`（中）/ `#58537B`（暗调）/ `#262540`（底）/ `#12111A`（压暗）。

---

## 界面位置

按钮都挂在行囊容器下，坐标相对 `adaptiveOffsetX / adaptiveOffsetY`：

| 控件 | 坐标 | 实际尺寸 | 说明 |
|---|---|---|---|
| 原生开关 | `9, 142` | 20×13 | 武器组切换 |
| 原生整理 | `37, 142` | 31×13 | `o_stackall` |
| 维修 | `54, 136` | 43×13 | 贴图 43×14，纵向压 1px（`13 / 14`）避免压到分隔线 |
| 仓库 | `96, 136` | 28×13 | 贴图 43×20，按 `28/43` × `13/20` 缩放 |

改位置只需动 `Gml/patch_buttons_create.gml` 里的两行 `scr_guiCreateInteractive(...)`。

两个按钮的父对象都是 `o_button_actionkey`（与整理按钮同源），所以悬停高亮、按下、点击音效、悬浮提示都走原生逻辑。改提示文案改各自 Create 里的 `hoverText`。

---

## 目录结构

```
Quick_Backpack/
├─ README.md                     本文件
├─ QuickBackpackMod.cs           Mod 入口：注册 2 个对象 + 11 处 GML 补丁
├─ Quick_Backpack.csproj         IDE 编译用（打包器不读 csproj）
├─ lib/                          内置引用 DLL（编译用）
│  ├─ ModShardLauncher.dll
│  └─ UndertaleModLib.dll
├─ icon.png                      模组卡片封面
├─ Gml/
│  ├─ o_qb_stash_btn_*.gml       仓库按钮（Create / 点击）
│  ├─ o_qb_repair_btn_*.gml      维修按钮（Create / 点击）
│  ├─ patch_buttons_create.gml   插入 o_inventory_Create_0：创建两个按钮
│  ├─ patch_stash_close.gml      替换 o_stash_inventory_left_Other_12 的关闭段
│  ├─ patch_repair_cost.gml           替换 o_inv_slot_Step_0 的维修费用行（交易分支）
│  ├─ patch_repair_cost_remote.gml    在 o_inv_slot_Step_0 的非交易分支补算维修费用
│  ├─ patch_repair_gold.gml           替换 Other_11 的扣费行
│  ├─ patch_repair_stolen.gml         替换 Other_20 的高亮判定行
│  ├─ patch_repair_material.gml       替换 Other_20 的材质范围行
│  ├─ patch_repair_ismaterial.gml     替换 Other_20 的材质开关行
│  ├─ patch_repair_canrepair.gml      替换 o_hoverWeapon 的可修判定行
│  ├─ patch_backpack_is_empty.gml     放行非空背包回行囊 / 在行囊内挪格子
│  └─ patch_can_use_backpack.gml      放行行囊内未打开的背包（让原生「打开」菜单项变亮）
└─ Textures/
   ├─ s_qb_stash_button_0/1/2.png    新精灵 43×20
   └─ s_qb_repair_button_0/1/2.png   新精灵 43×14
```

模组注册的对象：

| 对象 | 父对象 | 事件 | 用途 |
|---|---|---|---|
| `o_qb_stash_btn` | `o_button_actionkey` | Create / User Event 0 | 远程仓库按钮 |
| `o_qb_repair_btn` | `o_button_actionkey` | Create / User Event 0 | 维修按钮 |

---

## 构建与打包

> IDE 编译：本仓库内置 `lib/ModShardLauncher.dll`、`lib/UndertaleModLib.dll`（来自 ModShardLauncher Avalonia 版安装目录，默认 `%LOCALAPPDATA%\Programs\ModShardLauncher`，GPL-3.0 组件），`Quick_Backpack.csproj` 通过 `$(MSBuildThisFileDirectory)lib\...` 引用，克隆后无需安装启动器即可直接 `dotnet build Quick_Backpack.csproj`。这只产出 `bin/<Configuration>/net10.0/Quick_Backpack.dll`；实际生成 `.sml` 仍要用下面的 CLI。

```powershell
# 打包成 .sml（输出到 %LOCALAPPDATA%\ModShardLauncher\Mods）
ModShardLauncher.CLI.exe compile --source "%LOCALAPPDATA%\ModShardLauncher\ModSources\Quick_Backpack"

# 打补丁（必须用原版数据文件，不能叠加到已打过补丁的 data.win）
ModShardLauncher.CLI.exe patch `
    --data "C:\Program Files (x86)\Steam\steamapps\common\Stoneshard\original.win" `
    --output "C:\Program Files (x86)\Steam\steamapps\common\Stoneshard\data_qb.win" `
    --enable Quick_Backpack
```

CLI 退出码：`0` 成功 / `1` 参数或未启用 / `2` 编译或补丁失败 / `3` 未处理异常。

### 什么时候只 compile、什么时候必须 patch

| 改了什么 | 需要跑 |
|---|---|
| 只改了 `README.md` 等文档 | **只 compile**（`.sml` 里带上新文档即可，`.win` 不受影响） |
| 改了 `.gml` / `.cs` / `Textures\*.png` | compile + patch |
| 文档和代码都改了 | 全部改完，**最后跑一次** compile + patch |

要点：

- 一轮改动里不要「改代码 → 打包 → 改文档 → 再打包」，第二步的 `patch` 是重复的（还会白写一个 160 MB 文件）
- `patch` 永远基于 `original.win`，不要叠加到已有的输出文件上
- `.sml` 里打包了 README，所以分发时文档跟包走
- 重复写多个 `--enable` 只会生效最后一个，多模组要用逗号拼在一起

---

## 安装

1. 备份游戏目录的 `data.win`
2. 用启动器启用 `Quick_Backpack`，从**原版**数据文件打补丁
3. 将输出文件改名为 `data.win` 放回游戏目录

> 不要对已经打过补丁的 `data.win` 再次打补丁：日志注入会把 `o_msl_timer` 的 `Create_0` 重复添加而失败。游戏若使用 YYC 编译版本（非 `data.win`），本模组无法工作。

---

## 实现要点

### 不注册全局 GML 函数

`Msl.AddFunction` 只往 `Data.Code` 追加代码对象，汇编器编译调用时会往 `Data.Functions` 塞自定义函数条目（`gml_Script_scr_xxx`），而游戏自带的脚本函数并不在这张表里。调用落到这种条目上会**纯闪退、无报错弹窗**。

因此本模组所有逻辑与补丁片段都内联在对象事件 / 补丁代码里，不调用任何自定义函数。

### 补丁点

| 位置 | 作用 |
|---|---|
| `gml_Object_o_inventory_Create_0` | 行囊创建时挂上维修与仓库两个按钮 |
| `gml_Object_o_stash_inventory_left_Other_12` | 远程打开的仓库关闭时跳过「连带关掉行囊」 |
| `gml_Object_o_inv_slot_Step_0`（交易分支） | 维修费用系数在无商人时取 0.5 |
| `gml_Object_o_inv_slot_Step_0`（非交易分支） | 补齐远程维修的 `repair_cost`（原版只在 `if (_is_trade)` 里算，无商人时恒为 0 → 免费维修） |
| `gml_Object_o_skill_repair_item_master_Other_11` | 无商人时改用 `scr_gold_write_off(cost, undefined, true)` 扣费 |
| `gml_Object_o_skill_repair_item_master_Other_20` | 去掉偷窃判定；无商人时材质范围取 `["all"]`；补上 "all" 的材质判定（原版逐项比对字符串，不认 "all"） |
| `gml_Object_o_hoverWeapon_Other_20` | 武器提示里的可修判定改为恒真 |
| `gml_GlobalScript_scr_can_replace_item` | `scr_backpack_is_empty` 放行「已装备 / 刚拖起 / 已在行囊」且未打开的背包 |
| `gml_GlobalScript_scr_can_use_items` | 行囊内未打开的背包通过可用判定，使原生「打开」菜单项变亮 |

> `scr_backpack_is_empty` 是 `scr_can_replace_item.gml` 里的**嵌套函数**，补丁必须通过父条目 `gml_GlobalScript_scr_can_replace_item` 打（直接对 `gml_Script_scr_backpack_is_empty` 会报 `nested inside ... decompile that instead`）。

---

## 诊断日志

`%LOCALAPPDATA%\StoneShard\qb_diag.txt`

GameMaker 的文件函数受沙盒限制，只能写游戏存档目录，因此用相对文件名 `qb_diag.txt`。

记录内容：维修费用、仓库按钮点击、背包放回行囊等关键动作。正常操作不产生日志，只有失败/未命中才写。

---

## 已知限制

- 只对**玩家行囊**生效；交易、容器、仓库界面不会触发对应逻辑
- 远程维修不区分材质，全部可修；报价系数固定 0.5（铁匠铺仍取商人 `Repair_Prices`）
- 远程仓库无视距离，属有意设计
- 两个按钮用的是模组自建精灵（`s_qb_stash_button` / `s_qb_repair_button`），不改动游戏原有精灵
- 两个按钮的悬浮提示文案写死在各自 Create 的 `hoverText`（"维修装备" / "远程仓库"）
