// 整理按钮旁边的「维修」按钮：继承 o_button_actionkey（与整理按钮同一套悬浮提示逻辑）
event_inherited();

drawText = false;
text = "";
event = 0;

// 悬浮提示：与整理按钮相同的排版（显示在按钮上方）
hoverText = "维修装备";
hoverPlacementsArray = [1, 0, 0, -5];
enter_snd = -4;
click_snd = 1169;
owner_object = 4748;

// 贴图 43x14，但整排按钮的高度是 13（136..149），纵向压 1px 避免压到下方分隔线
image_xscale = 1;
image_yscale = 13 / 14;
