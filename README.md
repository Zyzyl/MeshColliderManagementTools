# MeshColliderManagementTools

This plugin adds a wizard for finding, replacing, or removing MeshCollider components in Neos world hierarchies. This also supports batch replacement / removal.


## Who is this for?
People to know the differences between collider component types, why one might want to avoid MeshColliders, and who wish to accelerate the collider-optimisation workflow. If you're unsure why this tool would be useful, ProbablePrime has some excellent information on colliders in their 3 part YouTube series on the topic. Part 3 (https://www.youtube.com/watch?v=xLVpzumI-H4) explains the differences between collider types.


## Why is this a plugin and not a tool in a public folder?
Currently we don't have access to the required functionality via LogiX or components in the main Neos build. In the long run I may consider rebuilding this tool inside Neos for easier distribution if future updates make that possible.


## How can I install / activate this plugin?
1. Download the `MeshColliderManagementTools.dll` file from this repository and place it in the 'Libraries' subfolder wherever NeosVR is installed on your PC.
2. Start Neos via the `NeosLauncher.exe` and make sure that the `MeshColliderManagementTools.dll` option is checked.
3. In a world where you have 'Builder' permissions, create an empty slot and attach the `MeshColliderManagementWiard` component from the 'Wizards' category. This will create a new wizard UI panel for you to use. The right side of the panel has a lot of empty space initially - this is intentional.


## How can I use the wizard?
First, drag a slot reference into the field below the 'Process root slot:' label. Any batch operations will use this slot and all child slots (to aribrary depth) as the target hierarchy.

### Filtering options:
- `Ignore inactive`: When checked, any batch operations will ignore MeshCollider components on slots which are inactive in the world hierarchy. This may be due to said slots being set inactive themselves, or one or more of their parent slots being set inactive.
- `Ignore disabled`: When checked, any batch operations will ignore disabled MeshCollider components.
- `Ignore non-persistent`: When checked, any batch operations will ignore MeshCollider components which are not marked as persistent. This may be due to the collider component itself being set non-persistent, it's containing slot being set non-persistent, or one or more of that slot's parents being set non-persistent.
- `Ignore user hierarchies`: When checked, any batch operations will ignore MeshCollider components which have an associated 'Active User' i.e. are present on a user's root slot or any children.
- `Tag`: If users input a string in this field it will be used to include / exclude MeshCollider components on slots with matching tags according to the `TagHandlingMode` setting.
- `TagHandlingMode`: An enum with 3 possible values: `IgnoreTag`, `IncludeOnlyWithTag`, `ExcludeAllWithTag`.
  - `IgnoreTag`: Any string value in the `Tag` field is ignored and no matching is performed during batch operations.
  - `IncludeOnlyWithTag`: Only MeshColliders on slots whose Tag exactly matches the string in the `Tag` field will be affected by batch operations.
  - `IncludeOnlyWithTag`: MeshColliders on slots whose Tag exactly matches the string in the `Tag` field will be ignored by batch operations.

MeshCollider components must be valid targets under all filtering options set in order for them to be affected by batch operations.

### Highlighing options:
- `Highlight duration`: A float value controlling how many seconds highlight visuals persist when a 'Higlight' button in the found collider list is pressed.
- `Highlight color`: Determines the color of the highlight visual.

### Collider replacement options:
- `Replacement collider component`: An enum with 3 possible values `BoxCollider`, `SphereCollider`, `ConvexHullCollider`. This controls which collider component type MeshColliders will be replaced with.
- `Replacement setup action`: An enum with 3 possible values `None`, `SetupFromLocalBounds`, `SetupFromGlobalBounds`. This controls whether new replacement BoxCollider or SphereCollider components will have their dimensions automatially setup using standard 'Setup from local bounds' or 'Setup from global bounds' functions available in the inspector panel for those components. Ignored when `Replacement collider component` is `ConvexHullCollider`.
- `Preserve existing collider settings`: When checked, new replacement collider components will have the 'Type', 'CharacterCollider', and 'IgnoreRaycasts' fields set to the same values as were on the replaced MeshCollider component. If unchecked these can be specified using the following options (hidden if this is checked):
  - `Set collision Type`: Determines the 'Type' enum value given to any new replacement colliders.
  - `Set CharacterCollider`: Determines the 'CharacterCollider' bool value given to any new replacement colliders.
  - `Set IgnoreRaycasts`: Determines the 'IgnoreRaycasts' bool value given to any new replacement colliders.

All of the above settings are respected by either batch or single component replacement actions.

### Batch actions:
- `List matching MeshColliders`: Builds a list of all MeshCollider components which are valid targets under the current filtering options. See below for more details.
- `Replace all matching MeshColliders`: Replaces all MeshCollider components which are valid targets under the current filtering options. This is undoable via the Undo / Redo radial menu buttons.
- `Remove all matching MeshColliders`: Deletes all MeshCollider components which are valid targets under the current filtering options. This is undoable via the Undo / Redo radial menu buttons.

Batch actions are applied to all MeshCollider components which are valid targets, given filtering options, at the moment the button is pressed. Note that this _may not_ be the same set which is listed if settings were changed since the `List maching MeshColliders` button was pressed! Using either the `Replace all matching MeshColliders` or `Remove all matching MeshColliders` will update the displayed list - typically emptying it.

### Single target actions for listed MeshColliders:
- `Jump To`: Moves the user who pressed the button close to the slot on which the relevant MeshCollider component is present.
- `Highlight`: Flash highlights the slot on which the relevant MeshCollider component is present respecting the `Highlight duration` and `Highlight color` settings.
- `Replace`: Replaces the relevant MeshCollider component respecting the current replacement settings. This is undoable via the Undo / Redo radial menu buttons.
- `Remove`: Deletes the relevent MeshCollider component. This is undoable via the Undo / Redo radial menu buttons.

Using either the `Replace` or `Remove` buttons will cause the displayed list of MeshColliders to be updated respecting current filtering options.


## How can users provide feedback?
I'm happy to receive any feedback (bug reports, feature requests, suggestions on improving my C# etc.) by pings for @Zyzyl#1441 in the official NeosVR Discord server or via direct message on Discord. If you prefer to use GitHub issues you could also log one of those in this repository. I make no promises as to implementing bugfixes or feature requests, but I want this to be a genuinely useful tool so they'll be seriously considered if I have the time.


Please do let me know if this has been helpful to you, it's always nice to hear and may help to motivate me to produce more tools in the future! This tool is provided free of charge, but if you wish to provide a small donation by way of thanks I'd very much appreciate NCR tips.
