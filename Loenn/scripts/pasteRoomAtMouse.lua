local utils = require("utils")
local map_item_utils = require("map_item_utils")
local loadedState = require("loaded_state")
local logger = require("logging")
local roomStruct = require("structs.room")

local script = {
    name = "pasteRoomAtMouse",
    displayName = "Paste Room At Mouse",
    minimumVersion = "1.0.3",
    tooltip = "Paste a room from your clipboard into the map at the position you click"
}

function script.run(room, args, ctx)
    local map = loadedState.map
    if map then
        local success, newRoom = utils.unserialize(love.system.getClipboardText())
        if success then
            newRoom = roomStruct.decode(newRoom)
            local duplicateCount = 1
            local name = newRoom.name
            while loadedState.getRoomByName(name) do
                name = string.format(newRoom.name .. " (%d)", duplicateCount)
                duplicateCount += 1
            end
            newRoom.name = name
            newRoom.x = ctx.mouseMapX
            newRoom.y = ctx.mouseMapY
            map_item_utils.addRoom(map, newRoom)
        end
    end
end

return script