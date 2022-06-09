local utils = require("utils")
local roomStruct = require("structs.room")

local script = {
    name = "copyRoom",
    displayName = "Copy Room",
    tooltip = "Copy the current room to your clipboard"
}

function script.run(room, args)
    local success, text = utils.serialize(roomStruct.encode(room), nil, nil, false)

    if success then
        love.system.setClipboardText(text)
    end
end

return script