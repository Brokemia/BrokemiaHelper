local fakeTilesHelper = require("helpers.fake_tiles")
local connectedEntities = require("helpers.connected_entities")
local utils = require("utils")
local matrixLib = require("utils.matrix")

local caveWall = {}
caveWall.name = "BrokemiaHelper/caveWall"
caveWall.depth = -13000
caveWall.placements = {
    name = "cave_wall",
    data = {
        tiletype = "3",
        disableTransitionFading = false,
        blockDisplacement = true,
        width = 8,
        height = 8
    }
}

caveWall.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

-- Filter all cave walls
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name
    end
end

function caveWall.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local firstEntity = relevantBlocks[1] == entity

    if firstEntity then
        -- Can use simple render, nothing to merge together
        if #relevantBlocks == 1 then
            return fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})(room, entity)
        end

        return fakeTilesHelper.getCombinedEntitySpriteFunction(relevantBlocks, "tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})(room)
    end

    local entityInRoom = utils.contains(entity, relevantBlocks)

    -- Entity is from a placement preview
    if not entityInRoom then
        return fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})(room, entity)
    end
end

return caveWall