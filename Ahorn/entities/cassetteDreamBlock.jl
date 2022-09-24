module CassetteSpaceJam

using ..Ahorn, Maple

@mapdef Entity "brokemiahelper/cassetteDreamBlock" CassetteDreamBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, fastMoving::Bool=false, oneUse::Bool=false, below::Bool=false, index::Integer=0, tempo::Number=1)

colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

const placements = Ahorn.PlacementDict(
    "Cassette Space Jam ($index - $color, BrokemiaHelper)" => Ahorn.EntityPlacement(
        CassetteDreamBlock,
        "rectangle",
        Dict{String, Any}(
            "index" => index,
        )
    ) for (color, index) in colorNames
)

Ahorn.nodeLimits(entity::CassetteDreamBlock) = 0, 1

Ahorn.minimumSize(entity::CassetteDreamBlock) = 8, 8
Ahorn.resizable(entity::CassetteDreamBlock) = true, true

Ahorn.editingOptions(entity::CassetteDreamBlock) = Dict{String, Any}(
    "index" => colorNames
)

#function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CassetteCar, room::Maple.Room)
#     # x, y = Ahorn.position(entity)

#     # width = 64
#     # height = 18

#     # # index = Int(get(entity.data, "index", 0))
#     # # color = get(colors, index, defaultColor)

#     # Ahorn.drawRectangle(ctx, 0, 0, width, height, defaultColor, defaultColor)
#     x = Int(get(entity.data, "x", 0))
#     y = Int(get(entity.data, "y", 0))

#     width = Int(get(entity.data, "width", 32))
#     height = Int(get(entity.data, "height", 32))

     #Ahorn.drawRectangle(ctx, 0, 0, 64, 18, (0.0, 0.0, 1.0, 0.4), (0.0, 0.0, 1.0, 1.0))
#end


function Ahorn.selection(entity::CassetteDreamBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    nodes = get(entity.data, "nodes", ())
    if isempty(nodes)
        return Ahorn.Rectangle(x, y, width, height)

    else
        nx, ny = Int.(nodes[1])
        return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
    end
end

colors = Dict{Integer, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

defaultColor = (73, 170, 240, 255) ./ 255

function renderCassetteSpaceJam(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number, index::Number)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)
    
    color = get(colors, index, defaultColor)
    Ahorn.drawRectangle(ctx, x, y, width, height, (0.0, 0.0, 0.0, 0.4), color)

    Ahorn.restore(ctx)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteDreamBlock)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    
    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        renderCassetteSpaceJam(ctx, nx, ny, width, height, Int(get(entity.data, "index", 0)))
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CassetteDreamBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderCassetteSpaceJam(ctx, 0, 0, width, height, Int(get(entity.data, "index", 0)))
end

borderMultiplier = (0.9, 0.9, 0.9, 1)

end
