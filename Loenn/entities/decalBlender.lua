local blendFunctionOptions = {
    "Add",
    "Subtract",
    "ReverseSubtract",
    "Max",
    "Min"
}

local blendOptions = {
    "One",
    "Zero",
    "SourceColor",
    "InverseSourceColor",
    "SourceAlpha",
    "InverseSourceAlpha",
    "DestinationColor",
    "InverseDestinationColor",
    "DestinationAlpha",
    "InverseDestinationAlpha",
    "BlendFactor",
    "InverseBlendFactor",
    "SourceAlphaSaturation"
}

return {
    name = "BrokemiaHelper/decalBlender",
    fieldInformation = {
        depth = {
            fieldType = "integer"
        },
        blendFactor = {
            fieldType = "color"
        },
        alphaBlendFunction = {
            options = blendFunctionOptions
        },
        colorBlendFunction = {
            options = blendFunctionOptions
        },
        alphaDestinationBlend = {
            options = blendOptions
        },
        alphaSourceBlend = {
            options = blendOptions
        },
        colorDestinationBlend = {
            options = blendOptions
        },
        colorSourceBlend = {
            options = blendOptions
        },
    },
    placements = {
        {
            name = "blenderBg",
            data = {
                width = 8,
                height = 8,
                decalNameFilter = "",
                alphaBlendFunction = "Max",
                alphaDestinationBlend = "One",
                alphaSourceBlend = "One",
                colorBlendFunction = "Max",
                colorDestinationBlend = "One",
                colorSourceBlend = "One",
                blendFactor = "ffffff",
                depth = 9000
            }
        },
        {
            name = "blenderFg",
            data = {
                width = 8,
                height = 8,
                decalNameFilter = "",
                alphaBlendFunction = "Max",
                alphaDestinationBlend = "One",
                alphaSourceBlend = "One",
                colorBlendFunction = "Max",
                colorDestinationBlend = "One",
                colorSourceBlend = "One",
                blendFactor = "ffffff",
                depth = -10500
            }
        }
    },
    fillColor = {0.2, 0, 0.8, 0.2},
    borderColor = {0.1, 0, 0.5, 0.4},
    depth = function(room, entity) return entity.depth end
}