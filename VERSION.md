# Version History
<hr/>

## Version 3.3.2
* Hotfix: Fixed application crash when performing panel selections when MSFS is not running.

## Version 3.3.1
* Added support to automatic disable Track IR during panel selection and panel pop out process.

## Version 3.3.0
* Pop out panel without a title bar can now be moved and resized.
* Added full screen mode capability to panel. This emulates MSFS Alt-Enter keystroke activation. To configure, just move the pop out panel to your desire monitor and select "Full Screen Mode" in the configuration grid for the panel. [Issue #13](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/13)
* Added automatic activation of profile when launching a flight session when a aircraft livery is bound to the profile.
* Multiple aircraft liveries can now be bound to a profile. An aircraft livery can only bind to a single profile. [Issue #16](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/16)
* Removed 'Set Default' profile function.
* Last used profile will be loaded when application starts.
* Added preference configuration to set Auto Panning custom view key binding. It is now possible to define key binding from Ctrl-Alt-0 through Ctrl-Alt-9 when saving cockpit custom camera view. [Issue #15](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/15)
* Added configuration to adjust delay for each of the auto pop out panel steps. 
* Added separate preference settings screen.
* Added auto update feature for future version of the application.

Bug Fixes:
* Application will go back to home screen correctly when a flight session ends.
* Application should reconnect to MSFS correctly when MSFS gracefully quits and restarts.

Known Issues:
* If a panel is in Full Screen Mode, using manual keystroke to return the panel to non-full screen mode will make the panel configuration data becomes out of sync. Restart and re-execute the pop out profile will solve the problem.
* Activating full screen mode, either through panel configuration grid or using manual Alt-Enter keystroke can only be done once per panel. This is a limitation on MSFS side. Subsequent Alt-Enter will only maximize the panel to full screen without stretching the content. Restart and re-execute the pop out profile will solve the problem.
* Hide Title Bar and Always on Top may not work if Full Screen Mode has been previously activated for a panel. This is a limitation on MSFS side on how windows are being handled.

## Version 3.2.0 (Beta)
* Added new Auto Pop Out Panels when flight start feature. Now the app will match a profile to the plane you're flying and perform all the pop outs for you, even help you to click the "ready to fly" button when a flying session is about to start!
* Added per monitor DPI-awareness support. The application should run and display correctly when using combination of mixed monitor (with high-DPI and low-DPI) resolutions and scaling.
* Added system tray icon access. Application can start minimize or minimize to system tray. System tray icon features a context menu to allow quick access to application functions.
* Added user requested feature to provide keyboard shortcut (Ctrl-Alt-P) to start panel pop out with either an active profile or a default profile selected.
* New copy profile feature. You can reuse your defined panel settings for another plane or plane/livery combination.
* Added quick panel location selection adjustment feature. You can now adjust panel locations without redoing the entire profile. 
* Added Save Auto Panning Camera Angle function if you need to adjust the in-game camera angle during panel selection.
* New logo icon for the app.
* New dark theme for the entire UI.
* Technical Note - Application is ported and rewritten with .NET WPF framework instead of WinForms.
## 
## Version 3.1.0.2 (Hotfix)
* Change application DPI mode to use DPIPerMonitor and added DPI aware setting to application to fix user configuration of using a 4K high DPI monitor (with windows scaling of greater than 100%) in conjunction of one or more lower DPI monitors such as 1440P or 1080P. With this configuration, the application rendering, panel selection and pop out panel adjustments and do not work correctly.

## Version 3.1.0
* Updated and streamlined UI to have a menu bar to control most application settings.
* Added long awaited auto save feature. Application no longer requires user to manually save profile after each panel change. All panel adjustments are saved automatically.
* Added panel lock feature to complement autosave feature. When panels are locked and are being moved or resized, their new location information will not get save. Also, for instrumentation pop out panels, when panels are locked, any accidental movement of the panels will return them to previously saved locations. For built-in panels such as ATC, VFR Map, etc, they can still be moved but their saved location will not get change.
* Added keyboard shortcuts for commonly use function. The buttons for -10px, -1px, +1px, and +10px now has keyboard shortcut of ‘Ctrl -’, ‘Ctrl [’, ‘Ctrl ]’, and ‘Ctrl +’ respectively.
* Added minimize all panels feature. This allows user to temporary see what is behind all open pop out panels. (This is a user requested feature with Github ticket [#6](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/6)).
* Various small bug fixes and usability enhancements.

## Version 3.0.1
* Added workaround for MSFS pop out panel adjustment bug when using the position data grid to adjust width and height will work as expected. 
	- In MSFS, when changing height or width of a pop out panel (2nd time for same panel and onward), there is an MSFS bug that will unexpectedly shifted the panel by 8px to the left for each adjustment.
* Improved real-time feedback of panel's current coordinates as you move panels around or adjust top, left, height and width of panel.
* Fixed always on top issue when moving panel around for placement inside other overlay or bezel.
* Added support to create profile just to save locations of built-in panels only (VFR, ATC, etc). 
* Added support to save locations for web panels from my other github project "MSFS Touch Panel".

## Version 3.0.0
* Provided 2X pop out and panel separation performance.
* Better support for all screen resolutions.
* Added Cold Start feature. Panels can be popped out and recalled later even when they're not turned on.
* Added Auto Panning feature. Application remembers the cockpit camera angle when you first define the pop out panels. It will automatically move the cockpit view for you when popping out panel.
* Added fine-grain control in positioning panels down to pixel level. 
* Added Always on Top feature for application.
* Added realtime readout during panel positioning.
* Added exception tracing to help troubleshoot application issue.

## Vesion 2.2.0
* Disabled ability to launch multiple instances of the application.
* Added autostart feature when MSFS starts. The application will create or modify exe.xml. A backup copy of exe.xml will be created.
* Added better support for 4K display resolution and non-standard display resolution.
* Windows OS display resolution and in-game display resolution no longer have to match. 
* Improved panel pop out separation accuracy and performance.
* Updated application packaging to single file executable to reduce file clutter.

## Vesion 2.1.1
* Fixed panel separation issue for super ultrawide monitor (for example: 3840x1080)

## Vesion 2.1.0
* Added ability to delete built-in profile.
* Added ability to create and delete custom user profile.
* Improved image recognition algorithm using SUSAN Corner block matching algorithm.

## Vesion 2.0.3
* Fixed a crash bug when splitting out panel when trying to analyze the last split panel.
* Added PMS50.com GTN750 mod configuration

## Vesion 2.0.2
* Added one second delay on mouse click when the application is trying to separate the chained pop out windows.

## Vesion 2.0.1
* Changed how screen resolution is detected. Used vertical instead of horizontal resolution to account for ultra wide monitors.

## Version 2.0.0
* Used new image recognition instead of OCR technology to determine pop outs.
* Added auto pop out feature.
* Allowed moving pop out panels using coordinates/width/height after analysis.
* Added additional plane profiles.
* Running on non-native monitor resolution will not work because of image scaling issue when doing image analysis.

## Version 1.2.0
* Increase OCR image accuracy by raising image DPI before analysis.
* Added (very experimental) Asobo A320 and FlybyWire A320NX profiles as testing sample. These profiles do only work 100% of the time. Continue investigation into better OCR accuracy will be needed.
* Added profile dropdown sorted by profile name.
* Fixed an issue of reapplying the same settings will cause panels to be out of sync.
* Fixed an issue of switching profiles will cause panels to be out of sync.
* Fixed an issue of unable to set or reset panel to NOT ALWAYS ON TOP.
* Fixed application path issue for not able to find ocrdata.json file at startup.
* Removed MSFS Pop Out Panel Manager is always on top. This is intefering with image operations. 

## Version 1.1.0
* Added caption title for the "untitled" windows. After analysis, if the panel window matches the name in the profile/ocr definition file, it will now display a caption of "Custom - XXXXX" (ie. Custom - PFD). This allows user to use various 3rd party windows layout manager to organize pop out panel windows.
* Added hide panel title bar feature.
* Added ability to have pop out panels to be always on top.
* Added minimize application to tray feature.
* Made application flow more intuitive.
* Fixed various small bugs in the application.

## Version 1.0.0
* Initial Release