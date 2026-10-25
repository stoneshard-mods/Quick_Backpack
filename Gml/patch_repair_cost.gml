repair_cost = max(1, math_round((0.35 + 0.05 * Tier) * (1 - _duration / _maxDuration) * base_price * (instance_exists(o_trade_inventory) ? o_trade_inventory.owner.Repair_Prices : 0.5)))
