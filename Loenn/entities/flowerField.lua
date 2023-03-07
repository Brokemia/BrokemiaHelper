local utils = require("utils")

return {
    name = "BrokemiaHelper/flowerField",
    fieldInformation = {
        depth = {
            fieldType = "integer"
        }
    },
    placements = {
        name = "field",
        data = {
            width = 8,
            randomFlipH = true,
            flowerDensity = 0.25,
            randomSeed = "",
            smallDandelions = true,
            largeDandelions = true,
            decals = "",
            depth = 9000
        }
    },
    color = {0, 0.8, 0, 0.5},
    rectangle = (room, entity -> utils.rectangle(entity.x, entity.y - 8, entity.width, 8))
}