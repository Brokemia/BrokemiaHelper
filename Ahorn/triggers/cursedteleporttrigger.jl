module BrokemiaHelperCursedTeleportTrigger

using ..Ahorn, Maple

@mapdef Trigger "brokemiahelper/cursedTeleportTrigger" CursedTeleportTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8, endRoom::String="")

const placements = Ahorn.PlacementDict(
    "CursedTeleportTrigger" => Ahorn.EntityPlacement(
        CursedTeleportTrigger,
        "rectangle",
        Dict{String, Any}()
    )
)

end
