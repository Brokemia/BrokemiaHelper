local utils = require("utils")

local presetFillColors = {
    green = {0.0, 0.6, 0.0, 0.7},
    red = {0.6, 0.0, 0.0, 0.7},
    blue = {0.0, 0.0, 0.6, 0.7},
    yellow = {0.6, 0.6, 0.0, 0.7},
    purple = {0.6, 0.0, 0.6, 0.7},
    cyan = {0.0, 0.6, 0.6, 0.7},
    white = {0.6, 0.6, 0.6, 0.7},
}

local do_it_for_the_vine = {}

do_it_for_the_vine.name = "BrokemiaHelper/vineinator"
do_it_for_the_vine.depth = 0
--do_it_for_the_vine.color = (room, entity -> presetFillColors[entity.preset or "green"])
do_it_for_the_vine.color = function(room, entity) return presetFillColors[entity.preset or "green"] end
do_it_for_the_vine.nodeLimits = {1, -1}
do_it_for_the_vine.nodeVisibility = "always"
do_it_for_the_vine.nodeLineRenderType = "line"

local function vinePresetData(preset, highlightColor, bodyColor, shadowColor, thornColor, hangingVineColor)
    return {
        seed = "",
        minSize = 3.0,
        maxSize = 8.0,
        sizeSpeed = 0.2,
        minHighlightProportion = 0.1,
        maxHighlightProportion = 0.6,
        highlightProportionSpeed = 0.2,
        maxLength = 100000,
        wanderStrength = 0.4,
        focusStrength = 0.2,
        homing = 0.25,
        homingDistance = 6.0,
        thorns = true,
        thornFrequency = 0.12,
        minThornSize = 1.0,
        maxThornSize = 2.0,
        thornShrinkSpeed = 0.5,
        hangingVines = true,
        hangingVineFrequency = 0.02,
        hangingVineSlack = 30,
        highlightColor = highlightColor,
        bodyColor = bodyColor,
        shadowColor = shadowColor,
        thornColor = thornColor,
        hangingVineColor = hangingVineColor,
        depth = 9000,
        preset = preset
    }
end
do_it_for_the_vine.placements = {
    {
        name = "vine",
        data = vinePresetData("green", "1a3236", "171f28", "040815", "1d5455", "040815")
    },
    {
        name = "vineRed",
        data = vinePresetData("red", "360b17", "210414", "110009", "5b162c", "110009")
    },
    {
        name = "vineBlue",
        data = vinePresetData("blue", "0048a1", "12335b", "010913", "1d7fbb", "010913")
    },
    {
        name = "vinePurple",
        data = vinePresetData("purple", "4a0b4a", "2a0a2a", "0f020f", "7f1d7f", "0f020f")
    },
    {
        name = "vineYellow",
        data = vinePresetData("yellow", "4a4a0b", "2a2a0a", "0f0f02", "7f7f1d", "0f0f02")
    },
    {
        name = "vineCyan",
        data = vinePresetData("cyan", "0b4a4a", "0a2a2a", "020f0f", "1d7f7f", "020f0f")
    },
    {
        name = "vineWhite",
        data = vinePresetData("white", "4a4a4a", "2a2a2a", "0f0f0f", "7f7f7f", "0f0f0f")
    }
}

do_it_for_the_vine.fieldOrder = {
    "x", "y",
    "depth",
    "seed",
    "maxLength",
    "highlightColor",
    "bodyColor",
    "shadowColor",
    "wanderStrength",
    "focusStrength",
    "homing",
    "homingDistance",
    "minSize",
    "maxSize",
    "sizeSpeed",
    "minHighlightProportion",
    "maxHighlightProportion",
    "highlightProportionSpeed",
    "thorns",
    "thornColor",
    "thornFrequency",
    "minThornSize",
    "maxThornSize",
    "thornShrinkSpeed",
    "hangingVines",
    "hangingVineColor",
    "hangingVineFrequency",
    "hangingVineSlack"
}

do_it_for_the_vine.ignoredFields = {"_id", "_name", "preset"}

do_it_for_the_vine.fieldInformation = {
    minSize = {
        minimumValue = 0.0
    },
    maxSize = {
        minimumValue = 0.0
    },
    sizeSpeed = {
        minimumValue = 0.0
    },
    minHighlightProportion = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    maxHighlightProportion = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    highlightProportionSpeed = {
        minimumValue = 0.0
    },
    maxLength = {
        fieldType = "integer",
        minimumValue = 1
    },
    wanderStrength = {
        minimumValue = 0.0
    },
    homingDistance = {
        minimumValue = 0.0
    },
    thornFrequency = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    minThornSize = {
        minimumValue = 0.0
    },
    maxThornSize = {
        minimumValue = 0.0
    },
    thornShrinkSpeed = {
        minimumValue = 0.0
    },
    hangingVineFrequency = {
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    hangingVineSlack = {
        fieldType = "integer"
    },
    highlightColor = {
        fieldType = "color"
    },
    bodyColor = {
        fieldType = "color"
    },
    shadowColor = {
        fieldType = "color"
    },
    thornColor = {
        fieldType = "color"
    },
    hangingVineColor = {
        fieldType = "color"
    },
    depth = {
        fieldType = "integer"
    }
}

function do_it_for_the_vine.rectangle(room, entity)
    return utils.rectangle(entity.x - 4, entity.y - 4, 8, 8)
end

return do_it_for_the_vine