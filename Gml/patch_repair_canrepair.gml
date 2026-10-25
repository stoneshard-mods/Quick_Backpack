_canRepair = (variable_instance_exists(id, "master_npc") ? (scr_array_get_index(master_npc.Material_Spec, _material) != -1 || scr_array_get_index(master_npc.Material_Spec, "all") != -1) : 1)
