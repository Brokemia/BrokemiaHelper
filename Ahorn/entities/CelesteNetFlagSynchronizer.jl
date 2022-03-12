module BrokemiaHelperCelesteNetFlagSynchronizer

using ..Ahorn, Maple

@mapdef Entity "BrokemiaHelper/CelesteNetFlagSynchronizer" CelesteNetFlagSynchronizer(x::Integer, y::Integer, flag::String="")


const placements = Ahorn.PlacementDict(
    "CelesteNet Flag Synchronizer (BrokemiaHelper)" => Ahorn.EntityPlacement(
        CelesteNetFlagSynchronizer,
        "point",
        Dict{String, Any}()
    )
)
sprite = "Ahorn/BrokemiaHelper/CelesteNetFlagSynchronizer.png"

function Ahorn.selection(entity::CelesteNetFlagSynchronizer)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CelesteNetFlagSynchronizer, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)



end
