module CassetteIntroCar

using ..Ahorn, Maple

@mapdef Entity "brokemiahelper/cassetteIntroCar" CarCassette(x::Integer, y::Integer, index::Integer=0, tempo::Number=1)

colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

const placements = Ahorn.PlacementDict(
    "Cassette Intro Car ($index - $color, BrokemiaHelper)" => Ahorn.EntityPlacement(
        CarCassette,
        "point",
        Dict{String, Any}(
            "index" => index,
        )
    ) for (color, index) in colorNames
)

Ahorn.editingOptions(entity::CarCassette) = Dict{String, Any}(
    "index" => colorNames
)

colors = Dict{Integer, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

defaultColor = (73, 170, 240, 255) ./ 255
borderMultiplier = (0.9, 0.9, 0.9, 1)

barrierSprite = "scenery/car/barrier"
bodySprite = "scenery/car/body"
pavementSprite = "scenery/car/pavement"
wheelsSprite = "scenery/car/wheels"

function Ahorn.selection(entity::CarCassette)
    x, y = Ahorn.position(entity)

    hasRoadAndBarriers = get(entity.data, "hasRoadAndBarriers", false)

    rectangles = Ahorn.Rectangle[
        Ahorn.getSpriteRectangle(bodySprite, x, y, jx=0.5, jy=1.0),
        Ahorn.getSpriteRectangle(wheelsSprite, x, y, jx=0.5, jy=1.0),
    ]

    if hasRoadAndBarriers
        push!(rectangles, Ahorn.getSpriteRectangle(barrierSprite, x + 32, y, jx=0.0, jy=1.0))
        push!(rectangles, Ahorn.getSpriteRectangle(barrierSprite, x + 41, y, jx=0.0, jy=1.0))
    end

    return Ahorn.coverRectangles(rectangles)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CarCassette, room::Maple.Room)
    x, y = Ahorn.position(entity)

    hasRoadAndBarriers = get(entity.data, "hasRoadAndBarriers", false)
    rng = Ahorn.getSimpleEntityRng(entity)

    pavementWidth = x - 48
    columns = floor(Int, pavementWidth / 8)

    Ahorn.drawSprite(ctx, wheelsSprite, x, y, jx=0.5, jy=1.0, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    Ahorn.drawSprite(ctx, bodySprite, x, y, jx=0.5, jy=1.0, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))

    if hasRoadAndBarriers
        Ahorn.drawSprite(ctx, barrierSprite, x + 32, y, jx=0.0, jy=1.0, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
        Ahorn.drawSprite(ctx, barrierSprite, x + 41, y, jx=0.0, jy=1.0, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))

        for col in 0:columns - 1
            choice = col >= columns - 2 ? (col != columns - 2 ? 3 : 2) : rand(rng, 0:2)

            Ahorn.drawImage(ctx, pavementSprite, col * 8, y, choice * 8, 0, 8, 4, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
        end
    end
end

end
