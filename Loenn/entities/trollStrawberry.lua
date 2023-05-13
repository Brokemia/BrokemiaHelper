local strawberry = require("utils").deepcopy(require("entities.strawberry"))

strawberry.name = "BrokemiaHelper/trollStrawberry"
for _, placement in pairs(strawberry.placements) do
    placement.data.reappear = false
end

return strawberry