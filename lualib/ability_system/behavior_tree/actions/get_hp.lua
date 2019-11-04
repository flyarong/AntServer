-- GetHp
--

local lualib_path = "ability_system.behavior_tree."
local bret = require(lualib_path .. "behavior_ret")

return function(node)
    local env = node.env
    return bret.SUCCESS, env.owner.hp
end
