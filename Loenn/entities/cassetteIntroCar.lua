local drawableSpriteStruct = require("structs.drawable_sprite")
local colors = require("consts.xna_colors")
local utils = require("utils")

local introCar = {}

local barrierTexture = "scenery/car/barrier"
local bodyTexture = "scenery/car/body"
local pavementTexture = "scenery/car/pavement"
local wheelsTexture = "scenery/car/wheels"

local colors = {
    {73 / 255, 170 / 255, 240 / 255},
    {240 / 255, 73 / 255, 190 / 255},
    {252 / 255, 220 / 255, 58 / 255},
    {56 / 255, 224 / 255, 78 / 255},
}

local depth = 1

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

introCar.name = "brokemiahelper/cassetteIntroCar"

introCar.fieldInformation = {
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
introCar.placements = {}

for i, _ in ipairs(colors) do
    introCar.placements[i] = {
        name = string.format("cassette_intro_car_%s", i - 1),
        data = {
            index = i - 1,
            tempo = 1.0,
            hasRoadAndBarriers = false,
            width = 16,
            height = 16
        }
    }
end

function introCar.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    local hasRoadAndBarriers = entity.hasRoadAndBarriers
    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]

    local bodySprite = drawableSpriteStruct.fromTexture(bodyTexture, entity)
    bodySprite:setJustification(0.5, 1.0)
    bodySprite:setColor(color)
    bodySprite.depth = depth

    local wheelSprite = drawableSpriteStruct.fromTexture(wheelsTexture, entity)
    wheelSprite:setJustification(0.5, 1.0)
    bodySprite:setColor(color)
    wheelSprite.depth = depth + 2

    table.insert(sprites, bodySprite)
    table.insert(sprites, wheelSprite)

    if hasRoadAndBarriers then
        utils.setSimpleCoordinateSeed(x, y)

        local pavementWidth = x - 48
        local columns = math.floor(pavementWidth / 8)

        for column = 0, columns - 1 do
            local choice = column >= columns - 2 and (column ~= columns - 2 and 3 or 2) or math.random(0, 2)
            local pavementSprite = drawableSpriteStruct.fromTexture(pavementTexture, entity)
            pavementSprite:addPosition(column * 8 - x, 0)
            pavementSprite.depth = -10001
            pavementSprite:setJustification(0.0, 0.0)
            pavementSprite:useRelativeQuad(choice * 8, 0, 8, 8)

            table.insert(sprites, pavementSprite)
        end

        local barrier1Sprite = drawableSpriteStruct.fromTexture(barrierTexture, entity)
        barrier1Sprite:addPosition(32, 0)
        barrier1Sprite:setJustification(0.0, 1.0)
        barrier1Sprite.depth = -10

        local barrier2Sprite = drawableSpriteStruct.fromTexture(barrierTexture, entity)
        barrier2Sprite:addPosition(41, 0)
        barrier2Sprite:setJustification(0.0, 1.0)
        barrier2Sprite.depth = 5
        barrier2Sprite.color = colors.DarkGray

        table.insert(sprites, barrier1Sprite)
        table.insert(sprites, barrier2Sprite)
    end

    return sprites
end

function introCar.selection(room, entity)
    local bodySprite = drawableSpriteStruct.fromTexture(bodyTexture, entity)
    local wheelSprite = drawableSpriteStruct.fromTexture(wheelsTexture, entity)

    bodySprite:setJustification(0.5, 1.0)
    wheelSprite:setJustification(0.5, 1.0)

    return utils.rectangle(utils.coverRectangles({bodySprite:getRectangle(), wheelSprite:getRectangle()}))
end

return introCar