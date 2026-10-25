// 点击：远程开关大篷车仓库（仓库数据在 global.caravanStashDataList* 里，与位置无关）
if (!instance_exists(o_stash_inventory_left))
{
    var _stash = scr_guiCreateContainer(global.guiBaseContainerSideLeft, o_stash_inventory_left);

    if (instance_exists(_stash))
    {
        // 标记：这个仓库是远程打开的，关闭时不要连带关掉行囊
        _stash.qb_remote = true;

        with (_stash)
            event_user(0);
    }
}
else
{
    with (o_stash_inventory_left)
        event_user(0);
}

var _lf = file_text_open_append("qb_diag.txt");
if (_lf != -1)
{
    file_text_write_string(_lf, string(current_time) + "  stash button clicked\n");
    file_text_close(_lf);
}
