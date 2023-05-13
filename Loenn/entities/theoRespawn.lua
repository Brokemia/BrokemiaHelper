local drawableSprite = require("structs.drawable_sprite")

local respawn = {}

respawn.name = "BrokemiaHelper/theoRespawn"
respawn.depth = 100
respawn.placements = {
    name = "respawn",
    data = {
        flag = ""
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function respawn.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY
    sprite:setColor({160 / 255, 160 / 255, 160 / 255, 80 / 255})

    return sprite
end

return respawn