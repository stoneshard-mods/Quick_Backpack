// 整理按钮（o_stackall @ 37,142，31x13 中心原点，占位 y 136..149）
// 右边的「维修」按钮：贴图 s_qb_repair_button 43x14（模组自建精灵），纵向按 13/14 压 1px，y 取 136 顶边对齐
qb_repair_button = scr_guiCreateInteractive(id, o_qb_repair_btn, depth - 3, 54 + adaptiveOffsetX, 136 + adaptiveOffsetY)

// 维修按钮右边的「远程仓库」按钮：贴图 s_qb_stash_button 43x20（美术稿按原 s_stash_button 画布），
// 显示时按 28/43 × 13/20 缩放进同一行
qb_stash_button = scr_guiCreateInteractive(id, o_qb_stash_btn, depth - 3, 96 + adaptiveOffsetX, 136 + adaptiveOffsetY)
