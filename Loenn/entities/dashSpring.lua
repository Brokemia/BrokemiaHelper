local utils = require("utils")

local springDepth = -8501
local springTexture = "objects/BrokemiaHelper/dashSpring/00"

local springUp = {}

springUp.name = "BrokemiaHelper/dashSpring"
springUp.depth = springDepth
springUp.justification = {0.5, 1.0}
springUp.texture = springTexture
springUp.selection = function(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 4, 12, 4)
end
springUp.placements = {
    name = "up",
    data = {
        spritePath = "objects/BrokemiaHelper/dashSpring/",
        playerCanUse = true,
        ignoreHoldables = false,
        ignoreRedBoosters = false
    }
}

local springDown = {}

springDown.name = "BrokemiaHelper/dashSpringDown"
springDown.depth = springDepth
springDown.justification = {0.5, 1.0}
springDown.texture = springTexture
springDown.rotation = math.pi
springDown.selection = function(room, entity)
    return utils.rectangle(entity.x - 6, entity.y, 12, 4)
end
springDown.placements = {
    name = "down",
    data = {
        spritePath = "objects/BrokemiaHelper/dashSpring/",
        playerCanUse = true,
        ignoreHoldables = false,
        ignoreRedBoosters = false
    }
}

local springLeft = {}

springLeft.name = "BrokemiaHelper/wallDashSpringLeft"
springLeft.depth = springDepth
springLeft.justification = {0.5, 1.0}
springLeft.texture = springTexture
springLeft.rotation = math.pi / 2
springLeft.selection = function(room, entity)
    return utils.rectangle(entity.x, entity.y - 6, 4, 12)
end
springLeft.placements = {
    name = "left",
    data = {
        spritePath = "objects/BrokemiaHelper/dashSpring/",
        playerCanUse = true,
        ignoreHoldables = false,
        ignoreRedBoosters = false
    }
}

local springRight = {}

springRight.name = "BrokemiaHelper/wallDashSpringRight"
springRight.depth = springDepth
springRight.justification = {0.5, 1.0}
springRight.texture = springTexture
springRight.rotation = -math.pi / 2
springRight.selection = function(room, entity)
    return utils.rectangle(entity.x - 4, entity.y - 6, 4, 12)
end
springRight.placements = {
    name = "right",
    data = {
        spritePath = "objects/BrokemiaHelper/dashSpring/",
        playerCanUse = true,
        ignoreHoldables = false,
        ignoreRedBoosters = false
    }
}

return {
    springUp,
    springDown,
    springRight,
    springLeft
}