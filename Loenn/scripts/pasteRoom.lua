local utils = require("utils")
local map_item_utils = require("map_item_utils")
local loadedState = require("loaded_state")
local logger = require("logging")
local roomStruct = require("structs.room")

local script = {
    name = "pasteRoom",
    displayName = "Paste Room",
    tooltip = "Paste a room from your clipboard into the map"
}

function script.run(room, args)
    local map = loadedState.map
    if map then
        local success, newRoom = utils.unserialize(love.system.getClipboardText())
        if success then
            newRoom = roomStruct.decode(newRoom)
            newRoom.name = "PASTED_ROOM"
            map_item_utils.addRoom(map, newRoom)
        end
    end
end

return script