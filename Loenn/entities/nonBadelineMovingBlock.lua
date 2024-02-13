local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")
local fakeTilesHelper = require("helpers.fake_tiles")

local movingBlock = {}

movingBlock.name = "BrokemiaHelper/nonBadelineMovingBlock"
movingBlock.depth = 0
movingBlock.nodeLineRenderType = "line"
movingBlock.nodeLimits = {1, -1}
movingBlock.fieldInformation = {
    surfaceSoundIndex = {
        fieldType = "integer"
    },
    easing = {
        options = {
            "Linear",
            "SineIn",
            "SineOut",
            "SineInOut",
            "QuadIn",
            "QuadOut",
            "QuadInOut",
            "CubeIn",
            "CubeOut",
            "CubeInOut",
            "QuintIn",
            "QuintOut",
            "QuintInOut",
            "ExpoIn",
            "ExpoOut",
            "ExpoInOut",
            "BackIn",
            "BackOut",
            "BackInOut",
            "BigBackIn",
            "BigBackOut",
            "BigBackInOut",
            "ElasticIn",
            "ElasticOut",
            "ElasticInOut",
            "BounceIn",
            "BounceOut",
            "BounceInOut"
        }
    },
    stopNode = {
        fieldType = "integer"
    },
}
-- Add the tile field stuff
movingBlock.fieldInformation = fakeTilesHelper.addTileFieldInformation(fakeTilesHelper.addTileFieldInformation(movingBlock.fieldInformation, "tiletype"), "highlightTiletype")
movingBlock.placements = {
    name = "moving_block",
    data = {
        tiletype="g",
        highlightTiletype="G",
        surfaceSoundIndex=8,
        startFlag="",
        startDelay=0.0,
        travelTime=0.8,
        perNodeTravelTimes="",
        stopNode=-1,
        easing="CubeIn",
        width = 8,
        height = 8
    }
}

function movingBlock.sprite(room, entity, viewport)
    return fakeTilesHelper.getEntitySpriteFunction(entity.tiletype, false)(room, entity, viewport)
end

function movingBlock.nodeSprite(room, entity, node, nodeIndex, viewport)
    return fakeTilesHelper.getEntitySpriteFunction(entity.highlightTiletype, false)(room, entity, node, nodeIndex, viewport)
end

function movingBlock.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
end

return movingBlock