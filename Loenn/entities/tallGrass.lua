local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")

return {
    name = "BrokemiaHelper/tallGrass",
    fieldInformation = {
        depth = {
            fieldType = "integer"
        }
    },
    placements = {
        name = "grass",
        data = {
            width = 8,
            grassTexture = "deco/BrokemiaHelper/grass",
            grassSpreadTexture = "deco/BrokemiaHelper/grassSpread",
            depth = -8500,
            wiggleSpeed = 3,
            wiggleFrequency = 1.5,
            minPlayerSpeed = 10,
            scaleMultiplier = 0.1,
            rotationMultiplier = 0.15
        }
    },
    minimumSize = function(room, entity)
        return atlases.getResource(entity.grassTexture, "Gameplay").width or 1, 1
    end,
    sprite = function(room, entity)
        local step = atlases.getResource(entity.grassTexture, "Gameplay").width or 1
    
        local sprites = {}
    
        for i = 0, entity.width - step, step do
            local sprite = drawableSprite.fromTexture(entity.grassTexture, {x = entity.x + i, y = entity.y})
            sprite:setJustification(0, 1)
    
            table.insert(sprites, sprite)
        end
    
        return sprites
    end
}