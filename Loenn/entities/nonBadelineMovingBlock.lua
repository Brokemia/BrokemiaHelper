local utils = require("utils")
local fakeTilesHelper = require("helpers.fake_tiles")

local movingBlock = {}

movingBlock.name = "BrokemiaHelper/nonBadelineMovingBlock"
movingBlock.depth = 0
movingBlock.nodeLineRenderType = "line"
movingBlock.nodeLimits = {1, -1}
movingBlock.fieldInformation = fakeTilesHelper.addTileFieldInformation(fakeTilesHelper.getFieldInformation("tiletype"), "highlightTiletype")
movingBlock.placements = {
    name = "moving_block",
    data = {
        tiletype="g",
        highlightTiletype="G",
        startFlag="",
        startDelay=0.0,
        travelTime=0.8,
        width = 8,
        height = 8
    }
}

movingBlock.sprite = function(room, entity, node)
    local materialKey = node and entity.tiletype or entity.highlightTiletype
    return fakeTilesHelper.getEntitySpriteFunction(materialKey, false)(room, entity, node)
end

return movingBlock