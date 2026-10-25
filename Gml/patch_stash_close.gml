if (!(variable_instance_exists(id, "qb_remote") && qb_remote))
{
    with (o_inventory)
    {
        closeLeftMenu = false;
        event_user(2);
        closeLeftMenu = true;
    }
}
