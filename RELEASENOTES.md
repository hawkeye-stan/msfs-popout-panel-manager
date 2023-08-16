## Version 4.0.2
* Added new logic to detect when flight session is ready to initiate pop out process for auto pop out panel.

* Updated logic to load custom camera view when performing pop out. The new logic is more reliable but unfornately will be a little slower because MSFS AAU2 may have introduced issue in loading and saving camera view. Pop out progress messages will show steps being taken when adjusting camera view.

* Added workaround for MSFS bug when using cockpit camera zoom setting is set with value other than 50 in MSFS general options. Pop out was failing before because saving and loading custom camera view does not work correctly in MSFS.

* Added workaround for CJ4 CDU panel not popping out because of MSFS bug.

* Added configurable keyboard shortcut to initiate pop out process (default is Ctrl-Shift-P). This keyboard shortcut can be configured in preference setting. This setting can be disabled to improve computing resource needed to constantly detect keyboard inputs.

* Fixed issue where full screen mode for pop out panel does not work on certain aircraft configuration.