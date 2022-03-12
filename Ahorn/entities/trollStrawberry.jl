module BrokemiaHelperTrollStrawberry

using ..Ahorn, Maple

@mapdef Entity "BrokemiaHelper/trollStrawberry" TrollStrawberry(x::Integer, y::Integer, winged::Bool=false, moon::Bool=false, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Troll Strawberry (BrokemiaHelper)" => Ahorn.EntityPlacement(
        TrollStrawberry
    ),
    "Troll Space Berry (BrokemiaHelper)" => Ahorn.EntityPlacement(
        TrollStrawberry,
        "point",
        Dict{String, Any}(
            "moon" => true
        )
    ),
    "Troll Strawberry (Winged) (BrokemiaHelper)" => Ahorn.EntityPlacement(
        TrollStrawberry,
        "point",
        Dict{String, Any}(
            "winged" => true
        )
    ),
)

# name, winged, has pips, moon
sprites = Dict{Tuple{String, Bool, Bool, Bool}, String}(
    ("BrokemiaHelper/trollStrawberry", false, false, false) => "collectables/strawberry/normal00",
    ("BrokemiaHelper/trollStrawberry", true, false, false) => "collectables/strawberry/wings01",
    ("BrokemiaHelper/trollStrawberry", false, true, false) => "collectables/ghostberry/idle00",
    ("BrokemiaHelper/trollStrawberry", true, true, false) => "collectables/ghostberry/wings01",

    ("BrokemiaHelper/trollStrawberry", false, false, true) => "collectables/moonBerry/normal00",
    ("BrokemiaHelper/trollStrawberry", true, false, true) => "collectables/moonBerry/ghost00",
    ("BrokemiaHelper/trollStrawberry", false, true, true) => "collectables/moonBerry/ghost00",
    ("BrokemiaHelper/trollStrawberry", true, true, true) => "collectables/moonBerry/ghost00",
)

fallback = "collectables/strawberry/normal00"

Ahorn.nodeLimits(entity::TrollStrawberry) = 0, -1

function Ahorn.selection(entity::TrollStrawberry)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())
    winged = get(entity.data, "winged", false)
    moon = get(entity.data, "moon", false)
    hasPips = length(nodes) > 0

    sprite = sprites[(entity.name, winged, hasPips, moon)]
    seedSprite = "collectables/strawberry/seed00"

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollStrawberry)
    x, y = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = node

        Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollStrawberry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())
    winged = get(entity.data, "winged", false)
    moon = get(entity.data, "moon", false)
    hasPips = length(nodes) > 0

    sprite = sprites[(entity.name, winged, hasPips, moon)]
    seedSprite = "collectables/strawberry/seed00"

    for node in nodes
        nx, ny = node

        Ahorn.drawSprite(ctx, seedSprite, nx, ny)
    end

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end