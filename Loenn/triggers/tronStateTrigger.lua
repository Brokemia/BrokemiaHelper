local trigger = {}

trigger.name = "BrokemiaHelper/tronStateTrigger"
trigger.fieldInformation = {
  color = {
    fieldType = "color"
  }
}
trigger.placements = {
  name = "trigger",
  data = {
      onlyOnce = false,
      maxSpeed = 190.0,
      targetSpeed = 140.0,
      slowSpeed = 91.0,
      color = "ffd65c",
  }
}

return trigger