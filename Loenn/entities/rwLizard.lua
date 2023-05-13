local utils = require("utils")

local lizard = {}

lizard.name = "BrokemiaHelper/rwLizard"
lizard.depth = -200
lizard.color = {0.0, 1.0, 0.0}
lizard.placements = {
    name = "lizard",
    data = {
    }
}

function lizard.rectangle(room, entity)
  return utils.rectangle(entity.x - 8, entity.y - 4, 16, 8)
end

return lizard