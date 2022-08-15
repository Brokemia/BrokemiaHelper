local questionable = {}

local flagOptions = {
    "XNAFNA",
    "VersionCeleste",
    "VersionEverest",
    "VersionEverestCeleste",
    "VersionEverestFull",
    "VersionEverestBuild",
    "VersionEverestSuffix",
    "VersionEverestTag",
    "VersionEverestCommit",
    "VersionBrokemiaHelper",
    "SystemMemoryMB",
    "PlayMode",
    "Platform"
}

questionable.name = "BrokemiaHelper/questionableFlagController"
questionable.depth = 0
questionable.texture = "Ahorn/BrokemiaHelper/questionableFlagController"
questionable.fieldInformation = {
    which = {
        options = flagOptions,
        editable = false
    }
}
questionable.placements = {
    name = "controller",
    data = {
        which = "XNAFNA",
        onlyOnce = false
    }
}

return questionable