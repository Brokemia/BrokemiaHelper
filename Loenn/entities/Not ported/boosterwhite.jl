module BoosterWhite

using ..Ahorn, Maple

@mapdef Entity "brokemiahelper/whitebooster" WhiteBooster(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Booster (White) (Brokemia Helper)" => Ahorn.EntityPlacement(
        WhiteBooster
    )
)

function boosterSprite(entity::WhiteBooster)
    return "objects/booster/booster00"
end

function Ahorn.selection(entity::WhiteBooster)
    x, y = Ahorn.position(entity)
    sprite = boosterSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WhiteBooster, room::Maple.Room)
    sprite = boosterSprite(entity)

    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
