local drawableSpriteStruct = require("structs.drawable_sprite")

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

local spinner = {}

spinner.name = "brokemiahelper/cassetteSpinner"

spinner.fieldInformation = {
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
spinner.placements = {}

for i, _ in ipairs(colors) do
    spinner.placements[i] = {
        name = string.format("cassette_spinner_%s", i - 1),
        data = {
            index = i - 1,
            tempo = 1.0,
            attachToSolid = false,
        }
    }
end

spinner.depth = -8500

function spinner.sprite(room, entity)
    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]

    -- Prevent color from spinner to tint the drawable sprite
    local position = {
        x = entity.x,
        y = entity.y
    }

    local texture = "danger/crystal/fg_white00"
    local sprite = drawableSpriteStruct.fromTexture(texture, position)
    sprite:setColor(color)

    return sprite
end

return spinner