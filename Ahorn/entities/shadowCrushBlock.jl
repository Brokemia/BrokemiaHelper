module ShadowCrushBlock

using ..Ahorn, Maple

@mapdef Entity "brokemiahelper/shadowCrushBlock" ShadowKevin(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, axes::String="both", chillout::Bool=false)

const placements = Ahorn.PlacementDict(
    "Shadow Kevin (Both, BrokemiaHelper)" => Ahorn.EntityPlacement(
        ShadowKevin,
        "rectangle"
    ),
    "Shadow Kevin (Vertical, BrokemiaHelper)" => Ahorn.EntityPlacement(
        ShadowKevin,
        "rectangle",
        Dict{String, Any}(
            "axes" => "vertical"
        )
    ),
    "Shadow Kevin (Horizontal, BrokemiaHelper)" => Ahorn.EntityPlacement(
        ShadowKevin,
        "rectangle",
        Dict{String, Any}(
            "axes" => "horizontal"
        )
    ),
)

const frameImage = Dict{String, String}(
    "none" => "objects/crushblock/block00",
    "horizontal" => "objects/crushblock/block01",
    "vertical" => "objects/crushblock/block02",
    "both" => "objects/crushblock/block03"
)

const smallFace = "objects/crushblock/idle_face"
const giantFace = "objects/crushblock/giant_block00"

const kevinColor = (98, 34, 43) ./ 255

Ahorn.editingOptions(entity::ShadowKevin) = Dict{String, Any}(
    "axes" => Maple.kevin_axes
)

Ahorn.minimumSize(entity::ShadowKevin) = 24, 24
Ahorn.resizable(entity::ShadowKevin) = true, true

Ahorn.selection(entity::ShadowKevin) = Ahorn.getEntityRectangle(entity)

# Todo - Use randomness to decide on Kevin border
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShadowKevin, room::Maple.Room)
    axes = lowercase(get(entity.data, "axes", "both"))
    chillout = get(entity.data, "chillout", false)

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    giant = height >= 48 && width >= 48 && chillout
    face = giant ? giantFace : smallFace
    frame = frameImage[lowercase(axes)]
    faceSprite = Ahorn.getSprite(face, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.drawRectangle(ctx, 2, 2, width - 4, height - 4, kevinColor)
    Ahorn.drawImage(ctx, faceSprite, div(width - faceSprite.width, 2), div(height - faceSprite.height, 2))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, 0, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, height - 8, 8, 24, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, 0, (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, width - 8, (i - 1) * 8, 24, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 24, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 24, 24, 8, 8)
end

end