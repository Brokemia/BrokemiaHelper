module BrokemiaHelperNonBadelineMovingBlock

using ..Ahorn, Maple

@pardef NonBadelineMovingBlock(x1::Integer, y1::Integer, x2::Integer=x1+16, y2::Integer=y1, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, tiletype::String="g", highlightTiletype::String="G", startFlag::String="", startDelay::Number=0.0, travelTime::Number=0.8) = Entity("BrokemiaHelper/nonBadelineMovingBlock", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, tiletype=tiletype, highlightTiletype=highlightTiletype, startFlag=startFlag, startDelay=startDelay, travelTime=travelTime)

const placements = Ahorn.PlacementDict(
    "Non-Badeline Boss Moving Block (BrokemiaHelper)" => Ahorn.EntityPlacement(
        NonBadelineMovingBlock,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    )
)

Ahorn.editingOptions(entity::NonBadelineMovingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "highlightTiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.nodeLimits(entity::NonBadelineMovingBlock) = 1, -1
Ahorn.minimumSize(entity::NonBadelineMovingBlock) = 8, 8
Ahorn.resizable(entity::NonBadelineMovingBlock) = true, true

# Code to support more than one node written by Catapillie

function Ahorn.selection(entity::NonBadelineMovingBlock)
    if entity.name == "BrokemiaHelper/nonBadelineMovingBlock"
        x, y = Ahorn.position(entity)
        nx, ny = Int.(entity.data["nodes"][1])

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))
        
        rects = [Ahorn.Rectangle(x, y, width, height)]

        for node in entity.data["nodes"]
            nx, ny = Int.(node)
            push!(rects, Ahorn.Rectangle(nx, ny, width, height))
        end

        return rects
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NonBadelineMovingBlock, room::Maple.Room)
    Ahorn.drawTileEntity(ctx, room, entity, material=get(entity.data, "tiletype", "g")[1], blendIn=false)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::NonBadelineMovingBlock, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    cox, coy = floor(Int, width / 2), floor(Int, height / 2)

    if !isempty(nodes)
        px, py = x, y
        for node in entity.data["nodes"]
	    nx, ny = Int.(node)
        
            # Use 'G' instead of 'g', as that is the highlight color of the block (the active color)
            fakeTiles = Ahorn.createFakeTiles(room, nx, ny, width, height, get(entity.data, "highlightTiletype", "G")[1], blendIn=false)
            Ahorn.drawFakeTiles(ctx, room, fakeTiles, room.objTiles, true, nx, ny, clipEdges=true)
            Ahorn.drawArrow(ctx, px + cox, py + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)

            px, py = nx, ny
        end
        Ahorn.drawArrow(ctx, px + cox, py + coy, x + cox, y + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
