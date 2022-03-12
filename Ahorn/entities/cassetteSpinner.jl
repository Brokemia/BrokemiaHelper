module CassetteSpeen

using ..Ahorn, Maple

@mapdef Entity "brokemiahelper/cassetteSpinner" CassetteSpinner(x::Integer, y::Integer, index::Integer=0, tempo::Number=1, attachToSolid::Bool=false)

colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

const placements = Ahorn.PlacementDict(
    "Cassette Spinner ($index - $color)" => Ahorn.EntityPlacement(
        CassetteSpinner,
        "point",
        Dict{String, Any}(
            "index" => index,
        )
    ) for (color, index) in colorNames
)

Ahorn.editingOptions(entity::CassetteSpinner) = Dict{String, Any}(
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

Ahorn.resizable(entity::CassetteSpinner) = false, false

function carSelection(entity::CassetteSpinner)
	rectangle = Ahorn.getEntityRectangle(entity)
	rectangle = Ahorn.Rectangle(rectangle.x-8, rectangle.y-8, 16, 16)
	return rectangle
end

Ahorn.selection(entity::CassetteSpinner) = carSelection(entity)

colors = Dict{Integer, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

defaultColor = (73, 170, 240, 255) ./ 255
borderMultiplier = (0.9, 0.9, 0.9, 1)

end
