local utils = require("utils")
local entities = require("entities")
local drawableSprite = require("structs.drawable_sprite")
local logging = require("logging")

local springDepth = -8501

local defaultSprites = {
    [true] = "objects/BrokemiaHelper/dashSpring/00",
    [false] = "objects/spring/00"
}

local rotations = {
    [0] = 0,
    [1] = math.pi / 2,
    [2] = -math.pi / 2,
    [3] = math.pi
}

local orientationNames = {
    ["Up"] = 0,
    ["Left"] = 1,
    ["Right"] = 2,
    ["Down"] = 3
}

local spring = {}

spring.name = "BrokemiaHelper/bigSpring"
spring.depth = springDepth
spring.justification = {0.5, 1.0}
spring.canResize = function(room, entity)
    local horizontal = entity.orientation == 0 or entity.orientation == 3
    return horizontal, not horizontal
end
spring.resize = function(room, entity, offsetX, offsetY, directionX, directionY)
    local canHorizontal, canVertical = entities.canResize(room, layer, entity)
    local minimumWidth = 8

    local oldWidth = entity.width or 0
    local newWidth = oldWidth

    if offsetX ~= 0 and canHorizontal then
        newWidth += offsetX * math.abs(directionX)

        if minimumWidth <= newWidth then
            entity.width = newWidth

            if directionX < 0 then
                entity.x -= offsetX
            end

            madeChanges = true
        end
    end

    if offsetY ~= 0 and canVertical then
        newWidth += offsetY * math.abs(directionY)

        if minimumWidth <= newWidth then
            entity.width = newWidth

            if directionY < 0 then
                entity.y -= offsetY
            end

            madeChanges = true
        end
    end

    return madeChanges
end
spring.updateResizeSelection = function(room, entity, node, selection, offsetX, offsetY, directionX, directionY)
    local newSelection = spring.selection(room, entity)

    selection.x = newSelection.x
    selection.y = newSelection.y
    selection.width = newSelection.width
    selection.height = newSelection.height
end
spring.rotation = function(room, entity)
    return rotations[entity.orientation]
end
spring.texture = function(room, entity)
    return (not entity.sprite or entity.sprite == "") and defaultSprites[entity.dashSpring] or (entity.sprite .. "00")
end
spring.scale = function(room, entity)
    local horizontal = entity.orientation == 0 or entity.orientation == 3
    local drawable = drawableSprite.fromTexture(spring.texture(room, entity), {x = entity.x, y = entity.y})
    if drawable then
        return (entity.width - entity.horizontalTexturePadding) / (drawable.meta.realWidth - entity.horizontalTexturePadding), 1.0
    end
    return 1.0, 1.0
end
spring.selection = function(room, entity)
    return ({
        [0] = utils.rectangle(entity.x - (entity.width - entity.horizontalTexturePadding) / 2, entity.y - 4, entity.width - entity.horizontalTexturePadding, 4),
        [1] = utils.rectangle(entity.x, entity.y - (entity.width - entity.horizontalTexturePadding) / 2, 4, entity.width - entity.horizontalTexturePadding),
        [2] = utils.rectangle(entity.x - 4, entity.y - (entity.width - entity.horizontalTexturePadding) / 2, 4, entity.width - entity.horizontalTexturePadding),
        [3] = utils.rectangle(entity.x - (entity.width - entity.horizontalTexturePadding) / 2, entity.y, entity.width - entity.horizontalTexturePadding, 4)
    })[entity.orientation]
end
spring.fieldInformation = {
    orientation = {
        fieldType = "integer",
        options = orientationNames,
        editable = false
    }
}

spring.placements = {
    {
        name = "up",
        data = {
            playerCanUse = true,
            orientation = 0,
            dashSpring = false,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "down",
        data = {
            playerCanUse = true,
            orientation = 3,
            dashSpring = false,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "left",
        data = {
            playerCanUse = true,
            orientation = 1,
            dashSpring = false,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "right",
        data = {
            playerCanUse = true,
            orientation = 2,
            dashSpring = false,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "dashUp",
        data = {
            playerCanUse = true,
            orientation = 0,
            dashSpring = true,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "dashDown",
        data = {
            playerCanUse = true,
            orientation = 3,
            dashSpring = true,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "dashLeft",
        data = {
            playerCanUse = true,
            orientation = 1,
            dashSpring = true,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    },
    {
        name = "dashRight",
        data = {
            playerCanUse = true,
            orientation = 2,
            dashSpring = true,
            width = 16,
            sprite = "",
            horizontalTexturePadding = 4
        }
    }
}

return spring