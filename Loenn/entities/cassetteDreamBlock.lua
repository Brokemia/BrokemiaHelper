local drawableSprite = require("structs.drawable_sprite")

local cassetteDB = {}

local colors = {
    {73 / 255, 170 / 255, 240 / 255},
    {240 / 255, 73 / 255, 190 / 255},
    {252 / 255, 220 / 255, 58 / 255},
    {56 / 255, 224 / 255, 78 / 255},
}

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

cassetteDB.name = "brokemiahelper/cassetteDreamBlock"
cassetteDB.nodeLineRenderType = "line"
cassetteDB.nodeLimits = {0, 1}

cassetteDB.fieldInformation = {
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
cassetteDB.placements = {}

for i, _ in ipairs(colors) do
    cassetteDB.placements[i] = {
        name = string.format("cassette_dream_block_%s", i - 1),
        data = {
            index = i - 1,
            tempo = 1.0,
            width = 16,
            height = 16
        }
    }
end

function cassetteDB.depth(room, entity)
    return entity.below and 5000 or -11000
end

cassetteDB.fillColor = {0.0, 0.0, 0.0}

function cassetteDB.borderColor(room, entity)
    local index = entity.index or 0
    return colors[index + 1] or colors[1]
end

return cassetteDB