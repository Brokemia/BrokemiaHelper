local persistent_mini_textbox_trigger_modes = {
  "OnPlayerEnter",
  "OnLevelStart",
  "OnWipeFinish",
  "OnTheoEnter"
}
local trigger = {}

trigger.name = "BrokemiaHelper/persistentMiniTextboxTrigger"
trigger.fieldInformation = {
  death_count = {
      fieldType = "integer",
  },
  mode = {
      options = persistent_mini_textbox_trigger_modes,
      editable = false
  }
}
trigger.placements = {
  name = "trigger",
  data = {
      dialog_id = "",
      mode = "OnPlayerEnter",
      only_once = true,
      death_count = -1,
      time = -1.0
  }
}

return trigger