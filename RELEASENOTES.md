## Version 4.0.2
* Added new logic to detect when flight session is ready to initiate pop out process for auto pop out panel.

* Fixed camera logic to save and load custom camera view used by Pop Out Panel Manager. This is to work around camera issue since AAU2. Also, pop out progress messages will now show steps being taken when adjusting camera view.

* Added workaround for MSFS bug when using cockpit camera zoom setting (POV) is set to value other than 50 in MSFS general options. Now, camera POV (zoom, height, horizontal position) can be freely adjusted without affecting how panel source definitions are configured.

* Added workaround for MSFS bug where CJ4 CDU panel does not pop out.

* Added new logic to configure panels if using camera options of Home Cockpit Mode. In this mode, since saving and loading of custom camera angle to define pop out panels is not available, new logic has been added to get this camera mode to work.

* Added configurable keyboard shortcut to initiate pop out process (default is Ctrl-Shift-O). This keyboard shortcut can be configured in preference setting. This setting can be disabled to improve computing resource needed to constantly detect keyboard inputs.

* Added indicator to show SimConnect is receiving data.

* Fixed issue where full screen mode for pop out panel does not work on certain aircraft configuration.