## Version 4.0.2
********* NOTE: The speed for Pop Out Panel Manager to execute pop outs will be slower than previous version because latest version MSFS had made existing POPM logic to not work reliably. Updated pop out logic requires wider timing threshold which unfortunately resulted in slower pop out speed. *********

* Added new logic to detect when flight session is ready to initiate pop out process for auto pop out panel.

* Updated logic to save and load custom camera view when performing pop out to workaround AAU2 issues. Pop out progress messages will now show steps being taken when adjusting camera view.

* Added workaround for MSFS bug when using cockpit camera zoom setting is set with value other than 50 in MSFS general options. Pop out was failing before because saving and loading custom camera view does not work correctly in MSFS.

* Added workaround for CJ4 CDU panel not popping out because of MSFS bug.

* Added configurable keyboard shortcut to initiate pop out process (default is Ctrl-Shift-O). This keyboard shortcut can be configured in preference setting. This setting can be disabled to improve computing resource needed to constantly detect keyboard inputs.

* Added separate logic to configure panels if using camera options of Home Cockpit Mode. Since in this mode, saving and loading of custom camera angle to define pop out panels is not avaible, new updated logic is needed. 

* Fixed issue where full screen mode for pop out panel does not work on certain aircraft configuration.