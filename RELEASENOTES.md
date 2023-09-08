## Version 4.0.2
* Added new logic (not based on timing) to detect when flight session is ready to initiate pop out process for auto pop out panel. You can also set Auto Pop Out Panel Delay in preferences to 0 seconds if you have a fast PC.

* Added failure state when custom camera view fails to load. Pop Out Panel Manager will try to load the user specified custom camera (Ctrl-Alt-X/Alt-X where X is the camera defined in preference setting) for 10 seconds and when it fails, POPM will no longer try to pop out panels.

* Added workaround for MSFS bug when using cockpit camera zoom setting (POV) is set to value other than 50 in MSFS general options. Now, camera POV (zoom, height, horizontal position) can be freely adjusted without affecting how panel source definitions are configured.

* Added workaround for MSFS bug where CJ4 CDU panel does not pop out.

* Added new logic to configure panels if using camera options of Home Cockpit Mode. In this mode, since saving and loading of custom camera angle to define pop out panels is not available, new logic has been added to get this camera mode to work.

* Added configurable keyboard shortcut to initiate pop out process (default is Ctrl-Shift-O). This keyboard shortcut can be configured in preference setting. This setting can be disabled to improve computing resource needed to constantly detect keyboard inputs.

* Updated camera logic to save and load custom camera view used by Pop Out Panel Manager. This is to work around camera issue since AAU2. Also, pop out progress messages will now show steps being taken when adjusting camera view.

* Fixed an issue where full screen mode for pop out panel does not work on certain aircraft configuration.

* Fixed an issue when manually closing pop out will reset profile panel configuration's width and height to 0 when the profile is unlocked. 

* Fixed logic where after panel source selection is completed, Pop Out Panel Manager will no longer recenter camera view. This is to remove confusion since Pop Out Manager has no way to tell the previous camera view you're on when panel source selection started.