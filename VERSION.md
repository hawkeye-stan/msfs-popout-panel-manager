# Version History
<hr/>

## Version 3.4

* Changed where user data files are stored. Previously, the files are saved in subfolder "userdata" in your installation directory. Now they're moved to your Windows "Documents" folder under "MSFS Pop Out Panel Manager" for easy access. When you first start the application, a data migration step will occur and your user data files will be moved to this new folder location. This change will allow you to install Pop Out Manager to any folder of your choice in your machine since the application no longer requires write access to your installation folder.

* Upgraded the application to use latest .NET 6.0 framework. You'll notice after user data migration, your installation folder will have only one executable file left.  Please don't be alarmed. This file is much bigger (67MB) since all .NET dependencies and application files are now packaged into a single file. You no longer need to manually install .NET framework to run this application.

* Revamped how Auto Pop Out Panel works. The app no longer tries to hunt and click "Ready to Fly" button by using "Ready to Fly Button Skipper" plugin I created. This community plugin is included in the installation folder and is required for Auto Pop Out Panel to work. Please copy the folder "zzz-ready-to-fly-button-skipper" in subfolder "community" in your installation location into your MSFS community folder. [(Issue #29)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/29).

* Updated how the application detects Sim Start and Sim Stop to perform Auto Pop Out. It now uses the much more reliable camera state SimConnect variable for detection.

* Fixed an issue where full screen mode panel does not expand the panel to fill the entire monitor. To use full screen mode correctly, once panel is popped out, move panel to desire monitor and adjust panel window to remove all top/bottom/left/right black bar. Then click on full screen mode checkbox for the panel. Next time when pop out is executed,  panel will be moved to correct monitor and it will expand to fill the entire screen.  [(Issue #27)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/27)

* Added a new feature to enable or disable fall back camera once pop out is completed. You can also define a custom camera view to load if centering cockpit view does not work for you. [(Issue #28)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/28)

* Added feature to change message dialog on screen duration. You can disable on screen message by setting the duration value to zero.

* (New for SU10+) Added additional keystroke option to pop out panel Ctrl + Right Ctrl + Left click instead of just Right-Alt Left click. This is designed for users that have a keyboard without the Right-Alt key. To use this feature, please map (CTRL + RIGHT CTRL) in Control Options => Miscellaneous => New UI Window Mode in the game.

* Changed how auto pop out with bound livery works. When you start or restart Pop Out Manager and if you're already in a flight with a bound profile, Pop Out Manager will automatic re-pop out all panels for you to sync up all settings based on your cold start or hot start configuration.

* Cleaned up UI and improved UI guidance for user by enable/disabling buttons and actions as needed.

* Lots of the code is rewritten from the ground up for better performance and stability. It also prepares the code architecture for v4.0 new features. Please be patient as I continue to implement v4.0. As always, please report issue and comment and I welcome all feedbacks and will do my best to fix or implement requested features.

Known Issues:

* In SU 10 beta (1.27.11.0), when using Touch Enabled and Full Screen Mode simultaneously, touch event may not register correctly. So if you want to use touch feature, please run the panel in regular pop out window mode instead of full screen.

## Version 3.3.7
* Fixed an issue where panel number circles are displayed at incorrect location instead of at the location where you clicked your mouse. This issue will most likely occur if your monitor display scale is set to greater than 100% in Windows display setting.

* Fixed an issue where panel number circles cannot be move immediately after the completion of panel selection.

* Added support for multi window (new in SU10 beta) so the app does not close the add-on window when pop out starts.

Known Issue:

* Currently, the time it takes for Auto Pop Out Panel to execute for G1000 based planes which require power on for cold start is much longer (can be up to 20 seconds) than previous version of the app. The reason is the time it takes to search and click the "Ready to Fly" button is much longer than before to account for various users' screen resolutions. A solution has already been created by me to totally skip the "Ready to Fly" button check and I'll incorporate this change into next major release of the application.

  https://flightsim.to/file/36500/ready-to-fly-button-skipper

## Version 3.3.6
* Hot Fix: Resolved an issue where panel separation fails if your MSFS game window is not on the same monitor as where the panels are initially popped out (upper left corner).

## Version 3.3.5
* Fixed an issue when using auto pop out panel in combination with power on during cold start for  the following two G1000 planes (Cessna 172 and Cessna 208B Grand Caravan), instrumentations are  not powering on to allow pop out to occur.
* Fixed an issue when panels are designated as full screen mode, they're not resizing to full screen after they're popped out.
* Fixed an issue when using auto pop out panel, "Ready to Fly" button may not get click when  interface scale is set to higher than 70. Unfortunately, because of how MSFS coded this particular button, this fix may add few extra seconds to the duration of auto pop out process since the application needs to search for the button to click. To speed up the pop out process, you can try to set auto pop out panel wait delays to minimum of 1 second in preferences menu and increase one second at a time until pop out works flawlessly for your system.
* Updated verbiage for "Save Auto Panning Camera" button to "Override Auto Panning Camera". This is to clear the confusion when initially selecting panels for a profile, clicking this button seems to be required. "Override Auto Panning Camera" is only needed when your camera viewport has changed for an existing profile and you do not want to recreate a new  profile to set new panel locations.
* Made improvement to the behavior of Track IR (enable/disable) when using the application.

## Version 3.3.4
* Fixed an issue when using Auto Pop Out Panel feature in conjunction with Auto Disable Track IR setting. When performing cold start on G1000 / G1000 NXi equipped plane with Power on required checked, PFD and MFD fail to turn on or they will turn off by themselves. This resulted in pop out process to fail.
* Fixed an issue when using Auto Pop Out Panel and the game is in Windows mode, Pop Out Manager fails to automatically click the "Ready to Fly" button.
* Made improvements to the detection of flight start and flight end. This help to resolve an issue when exiting a flight, Pop Out Manager tries to pop out panels again.
* Added touch enabled panel experimental feature. Please see github repo README.md on how to use this feature. This feature tries to workaround an outstanding issue regarding lack of support by MSFS with pop out that has touch component (GTN750, King Air 350, Built-in panels such as Check List, ATC, etc).
* Updated documentations and how to videos.

## Version 3.3.3
* Fixed an issue when clicking on "Show/Edit Panel Location Overlay" or setting auto Track IR disabling option will cause PFD/MFD panels to be turned off when performing auto pop out in cold start for G1000 equipped planes.
* Fixed an issue where auto panning of cockpit view does not pan to previously saved camera view during pop out process.

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