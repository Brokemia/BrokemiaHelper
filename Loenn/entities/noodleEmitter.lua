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
        noodleBloodthirst = {
            minimumValue = 0.0,
            maximumValue = 1.0
        }
    },
    nodeLimits = {1, -1},
    nodeLineRenderType = "fan",
    placements = {
        name = "emitter",
        data = {
            depth = -8500,
            seed = 0,
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
            noodleBloodthirst = 0
        }
    },
    rectangle = function(room, entity) return utils.rectangle(entity.x, entity.y, 8, 8) end
}