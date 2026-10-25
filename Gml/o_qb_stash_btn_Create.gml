// 行囊工具栏的「远程仓库」按钮：继承 o_button_actionkey（与整理按钮同一套悬浮提示逻辑）
event_inherited();

drawText = false;
text = "";
event = 0;

// 悬浮提示：与整理按钮相同的排版（显示在按钮上方）
hoverText = "远程仓库";
hoverPlacementsArray = [1, 0, 0, -5];
enter_snd = -4;
click_snd = 1169;
owner_object = 4748;

// 贴图是模组自己的新精灵 s_qb_stash_button（43x20，美术稿按原 s_stash_button 的画布尺寸绘制），
// 工具栏行高只有 13，所以按 13/20 缩放显示；不再借用游戏原有的 s_stash_button，
// 免得影响 o_stash_button 等原生界面。
image_xscale = 28 / 43;
image_yscale = 13 / 20;
