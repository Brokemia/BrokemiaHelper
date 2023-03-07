local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local logging = require("logging")
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
            minPlayerSpeed = 100,
            scaleMultiplier = 0.1,
            rotationMultiplier = 0.15
        }
    },
    --color = {0, 0.5, 0, 0.5},
    --rectangle = (room, entity -> utils.rectangle(entity.x, entity.y - 8, entity.width, 8))
    sprite = function(room, entity)
        -- TODO: get step from texture
        local step = atlases.getResource(entity.grassTexture, "Gameplay").width or 1
    
        local sprites = {}
    
        for i = 0, entity.width - step, step do
            local sprite = drawableSprite.fromTexture(entity.grassTexture, {x = entity.x + i, y = entity.y})
            logging.info(entity.grassTexture)
            sprite:setJustification(0, 1)
    
            table.insert(sprites, sprite)
        end
    
        return sprites
    end
}