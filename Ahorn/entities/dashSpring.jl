module BrokemiaHelperDashSpring

using ..Ahorn, Maple

@mapdef Entity "BrokemiaHelper/dashSpring" DashSpring(x::Integer, y::Integer, playerCanUse::Bool=true)
@mapdef Entity "BrokemiaHelper/dashSpringDown" DashSpringDown(x::Integer, y::Integer, playerCanUse::Bool=true)
@mapdef Entity "BrokemiaHelper/wallDashSpringRight" DashSpringRight(x::Integer, y::Integer)
@mapdef Entity "BrokemiaHelper/wallDashSpringLeft" DashSpringLeft(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Dash Spring (Up, Brokemia Helper)" => Ahorn.EntityPlacement(
        DashSpring
    ),
    "Dash Spring (Down, Brokemia Helper)" => Ahorn.EntityPlacement(
        DashSpringDown
    ),
    "Dash Spring (Left, Brokemia Helper)" => Ahorn.EntityPlacement(
        DashSpringRight
    ),
    "Dash Spring (Right, Brokemia Helper)" => Ahorn.EntityPlacement(
        DashSpringLeft
    ),
)

function Ahorn.selection(entity::DashSpring)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
end

function Ahorn.selection(entity::DashSpringDown)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y, 12, 5)
end

function Ahorn.selection(entity::DashSpringLeft)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
end

function Ahorn.selection(entity::DashSpringRight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
end

sprite = "objects/BrokemiaHelper/dashSpring/00.png"

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashSpring, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -8)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashSpringDown, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 16, 24, rot=pi)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashSpringLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 24, 0, rot=pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashSpringRight, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, -8, 16, rot=-pi / 2)

end
