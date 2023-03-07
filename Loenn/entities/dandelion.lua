return {
    name = "BrokemiaHelper/dandelion",
    fieldInformation = {
        seeds = {
            fieldType = "integer",
            minimumValue = 0
        },
        depth = {
            fieldType = "integer"
        }
    },
    justification = {0.5, 1},
    scale = function(room, entity) return (entity.flipH ? -1 : 1), (entity.flipV ? -1 : 1) end,
    placements = {
        {
            name = "small",
            data = {
                seeds = 5,
                seedStartX = 0,
                seedStartY = -5,
                seedTexture = "deco/BrokemiaHelper/dandelion/seed",
                flyAwayWidth = 8,
                flyAwayHeight = 8,
                flipH = false,
                flipV = false,
                sprite = "BrokemiaHelper_DandelionSmall",
                depth = 9000
            }
        },
        {
            name = "large",
            data = {
                seeds = 5,
                seedStartX = 0,
                seedStartY = -8,
                seedTexture = "deco/BrokemiaHelper/dandelion/seed",
                flyAwayWidth = 8,
                flyAwayHeight = 12,
                flipH = false,
                flipV = false,
                sprite = "BrokemiaHelper_DandelionLarge",
                depth = 9000
            }
        }
    },
    texture = function(room, entity)
        return "deco/BrokemiaHelper/dandelion/small00"
    end
}