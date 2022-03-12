module BrokemiaHelperMoveBlockBarrier

using ..Ahorn, Maple

@mapdef Entity "BrokemiaHelper/moveBlockBarrier" MoveBlockBarrier(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, blockDebris::Bool=false)

const placements = Ahorn.PlacementDict(
    "Move Block Barrier (BrokemiaHelper)" => Ahorn.EntityPlacement(
        MoveBlockBarrier,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::MoveBlockBarrier) = 8, 8
Ahorn.resizable(entity::MoveBlockBarrier) = true, true

function Ahorn.selection(entity::MoveBlockBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MoveBlockBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
    
    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.45, 0.0, 0.45, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end