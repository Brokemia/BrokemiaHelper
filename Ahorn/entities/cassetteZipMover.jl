module CassetteZipper

using ..Ahorn, Maple

@pardef CassetteZipMover(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, width::Integer=8, height::Integer=8, theme="Normal", index::Integer=0, tempo::Number=1) = Entity("brokemiahelper/cassetteZipMover", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, theme=theme, index=index, tempo=tempo)

colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

const placements = Ahorn.PlacementDict(
    "Cassette Zip Mover ($index - $color, BrokemiaHelper)" => Ahorn.EntityPlacement(
        CassetteZipMover,
        "rectangle",
        Dict{String, Any}(
            "index" => index,
            "theme" => "Normal"
        ),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    ) for (color, index) in colorNames
)

Ahorn.nodeLimits(entity::CassetteZipMover) = 0, 1

Ahorn.minimumSize(entity::CassetteZipMover) = 16, 16
Ahorn.resizable(entity::CassetteZipMover) = true, true

Ahorn.editingOptions(entity::CassetteZipMover) = Dict{String, Any}(
    "index" => colorNames,
    "theme" => Maple.zip_mover_themes
)


colors = Dict{Integer, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

defaultColor = (73, 170, 240, 255) ./ 255

borderMultiplier = (0.9, 0.9, 0.9, 1)

function Ahorn.selection(entity::CassetteZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
end

function getTextures(entity::CassetteZipMover)
    theme = lowercase(get(entity, "theme", "normal"))
    
    if theme == "moon"
        return "objects/zipmover/moon/block", "objects/zipmover/moon/light01", "objects/zipmover/moon/cog"
    end

    return "objects/zipmover/block", "objects/zipmover/light01", "objects/zipmover/cog"
end

ropeColor = (102, 57, 49) ./ 255

function renderCassetteZipMover(ctx::Ahorn.Cairo.CairoContext, entity::CassetteZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    block, light, cog = getTextures(entity)
    lightSprite = Ahorn.getSprite(light, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    cx, cy = x + width / 2, y + height / 2
    cnx, cny = nx + width / 2, ny + height / 2

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, ropeColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    # Offset for rounding errors
    Ahorn.move_to(ctx, 0, 4 + (theta <= 0))
    Ahorn.line_to(ctx, length, 4 + (theta <= 0))

    Ahorn.move_to(ctx, 0, -4 - (theta > 0))
    Ahorn.line_to(ctx, length, -4 - (theta > 0))

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)

    Ahorn.drawRectangle(ctx, x + 2, y + 2, width - 4, height - 4, (0.0, 0.0, 0.0, 1.0))
    Ahorn.drawSprite(ctx, cog, cnx, cny, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y, 8, 0, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, block, x, y + (i - 1) * 8, 0, 8, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
        Ahorn.drawImage(ctx, block, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    end

    Ahorn.drawImage(ctx, block, x, y, 0, 0, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    Ahorn.drawImage(ctx, block, x + width - 8, y, 16, 0, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    Ahorn.drawImage(ctx, block, x, y + height - 8, 0, 16, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))
    Ahorn.drawImage(ctx, block, x + width - 8, y + height - 8, 16, 16, 8, 8, tint=get(colors, Int(get(entity.data, "index", 0)), defaultColor))

    Ahorn.drawImage(ctx, lightSprite, x + floor(Int, (width - lightSprite.width) / 2), y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteZipMover, room::Maple.Room)
    renderCassetteZipMover(ctx, entity)
end

end
