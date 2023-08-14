## Version 4.0.2
* Added new logic to detect when flight session is ready to initiate pop out process. Ready to Fly delay setting is no longer needed. Please install the updated "ready-to-fly-button-skipper" community plugin to your community folder to shorten the time when pop out process starts. The old version waits 2 seconds before ready to fly button is deactivated and the new version is immediate.

* Updated logic to load custom camera view when performing pop out. It is now more reliable and reduces unnecessary shifting of camera angle before starting pop out process.

* Added workaround for CJ4 CDU panel not popping out because of MSFS bug.

* Added workaround fix when using camera zoom setting with value other than 50 in MSFS general options will cause Pop Out Panel Manager pop out to fail. This is an existing MSFS bug where saving and loading of custom camera view is currently broken for zoom level other than 50.

* Add configurable keyboard shortcut to initiate pop out process (default is Ctrl-Shift-P). This keyboard shortcut can be configured in preference setting. This setting can be disabled to improve computing resource needed to constantly detect keyboard inputs.

* Fixed issue where full screen mode for pop out panel does not work on certain aircraft configuration.