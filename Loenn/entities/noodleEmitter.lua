local utils = require("utils")
return {
    name = "BrokemiaHelper/noodleEmitter",
    fieldInformation = {
        depth = {
            fieldType = "integer"
        },
        seed = {
            fieldType = "integer"
        },
        noodleDepth = {
            fieldType = "integer"
        },
        noodleLength = {
            fieldType = "integer"
        },
        noodleCount = {
            fieldType = "integer"
        },
        noodleBloodthirst = {
            minimumValue = 0.0,
            maximumValue = 1.0
        },
    },
    nodeLimits = {1, -1},
    nodeLineRenderType = "fan",
    placements = {
        name = "emitter",
        data = {
            sprite = "BrokemiaHelper_NoodleEmitter_A",
            editorTexture = "objects/BrokemiaHelper/noodleEmitter/A/idle00",
            depth = -8500,
            seed = 0,
            noodleCount = 4,
            noodleLimit = 8,
            minEmitTime = 1,
            maxEmitTime = 6,
            noodleDepth = -13010,
            noodleLength = 15,
            noodleFastSpeed = 70,
            noodleSlowSpeed = 30,
            noodleAcceleration = 50,
            noodleTailTaper = 0.5,
            noodleWanderStrength = 0.4,
            noodlePlayerInterest = 0.07,
            noodleFriendInterest = 0.02,
            noodleHomingDistance = 6,
            noodleHoming = 0.25,
            noodleJourneyMax = 15,
            noodleJourneyMin = 4,
            noodleFocusMax = 0.15,
            noodleFocusIncrease = 0.02,
            noodleSightDistance = 64,
            noodleBloodthirst = 0,
            noodleColorGradient = true,
            noodleHeadColor = "800080",
            noodleTailColor = "9370DB",
            noodleGenetics = false
        }
    },
    texture = function(room, entity) return entity.editorTexture end,
    nodeColor = {1, 1, 1, 0.2},
    --nodeRectangle = function(room, entity, node, nodeIndex, viewport) return utils.rectangle(node.x - 2, node.y - 2, 4, 4) end,
    nodeTexture = "collectables/strawberry/seed00",
}