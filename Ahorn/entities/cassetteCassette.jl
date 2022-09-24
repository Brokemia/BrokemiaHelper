module BrokemiaCassetteCassette

using ..Ahorn, Maple

@pardef CassetteCassette(x1::Integer, y1::Integer, x2::Integer=x1, y2::Integer=y1, index::Integer=0, tempo::Number=1) = Entity("brokemiahelper/cassetteCassette", x=x1, y=y1, nodes=Tuple{Int, Int}[(0, 0), (x2, y2)], index=index, tempo=tempo)

colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

const placements = Ahorn.PlacementDict(
    "Cassette Cassette ($index - $color, BrokemiaHelper)" => Ahorn.EntityPlacement(
        CassetteCassette,
        "point",
        Dict{String, Any}(
            "index" => index,
        ),
        function(entity)
            entity.data["nodes"] = [
                (Int(entity.data["x"]) + 32, Int(entity.data["y"])),
                (Int(entity.data["x"]) + 64, Int(entity.data["y"]))
            ]
        end
    ) for (color, index) in colorNames
)

Ahorn.editingOptions(entity::CassetteCassette) = Dict{String, Any}(
    "index" => colorNames
)

Ahorn.nodeLimits(entity::CassetteCassette) = 2, 2
sprite = "collectables/cassette/idle00.png"

colors = Dict{Integer, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

defaultColor = (73, 170, 240, 255) ./ 255
borderMultiplier = (0.9, 0.9, 0.9, 1)

function Ahorn.selection(entity::CassetteCassette)
    x, y = Ahorn.position(entity)
    controllX, controllY = Int.(entity.data["nodes"][1])
    endX, endY = Int.(entity.data["nodes"][2])

    return [
        Ahorn.getSpriteRectangle(sprite, x, y),
        Ahorn.getSpriteRectangle(sprite, controllX, controllY),
        Ahorn.getSpriteRectangle(sprite, endX, endY)
    ]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteCassette)
    px, py = Ahorn.position(entity)
    nodes = entity.data["nodes"]

    for node in nodes
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
        px, py = nx, ny
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CassetteCassette, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))



end
