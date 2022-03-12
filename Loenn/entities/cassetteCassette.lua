local drawableSprite = require("structs.drawable_sprite")

local cassetteCassette = {}

local colors = {
    {73 / 255, 170 / 255, 240 / 255},
    {240 / 255, 73 / 255, 190 / 255},
    {252 / 255, 220 / 255, 58 / 255},
    {56 / 255, 224 / 255, 78 / 255},
}

local depth = -1000000

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

cassetteCassette.name = "brokemiahelper/cassetteCassette"
cassetteCassette.nodeLineRenderType = "line"
cassetteCassette.nodeLimits = {2, 2}

cassetteCassette.fieldInformation = {
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
cassetteCassette.placements = {}

for i, _ in ipairs(colors) do
    cassetteCassette.placements[i] = {
        name = string.format("cassette_cassette_%s", i - 1),
        data = {
            index = i - 1,
            tempo = 1.0,
            width = 16,
            height = 16
        }
    }
end

function cassetteCassette.sprite(room, entity)
    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]

    local sprite = drawableSprite.fromTexture("collectables/cassette/idle00", entity)

    sprite:setColor(color)

    sprite.depth = depth

    return sprite
end

return cassetteCassette