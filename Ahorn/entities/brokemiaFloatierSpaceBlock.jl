module BrokemiaHelperFloatierSpaceBlock

using ..Ahorn, Maple

@mapdef Entity "BrokemiaHelper/floatierSpaceBlock" FloatierSpaceBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, tiletype::String="3", disableSpawnOffset::Bool=false, floatinessMultiplier::Number=1.0, bounceBackMultiplier::Number=1.0, dashOffsetMultiplier::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Floatier Space Block (BrokemiaHelper)" => Ahorn.EntityPlacement(
        FloatierSpaceBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::FloatierSpaceBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FloatierSpaceBlock) = 8, 8
Ahorn.resizable(entity::FloatierSpaceBlock) = true, true

Ahorn.selection(entity::FloatierSpaceBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FloatierSpaceBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end