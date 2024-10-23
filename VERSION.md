# Version History
<hr/>

## Version 4.1.2

* Added touch support for pop out when in full screen mode.

* Fixed pop out always on top issue where it sometimes does not work.

* Improved dynamic LOD FPS detection and logic.


## Version 4.1.1

* Added option to store POPM profiles and configuration files in your user's AppData Roaming folder instead of Documents folder. Hopefully, this solved the issue where OneDrive users are having issue with POPM files.

* Fixed POPM inability to close correctly when Keyboard Shortcuts preference is disabled.

* Fixed issue where auto start option failed to retain CommandLine arguments in exe.xml file.

* Fixed issue where auto start does not work correctly for Steam version of MSFS since exe.xml file location has been moved by Steam installation.

* Added ability for full screen panel to work as floating panel. An example use case is to show and hide EFB as full screen using keyboard shortcut.

* Added dynamic LOD (my own implementation of AutoFPS) - this is totally experimental and unsupported. If you decide to use this version of dynamic LOD, you don't have to run multiple apps.

* Fixed various smaller bugs.

## Version 4.1.0

* Added new method to select panel source for an aircraft profile using fixed camera view instead of relying saved custom camera view. Previous method of using saved custom camera view is still available to use if desire.

Video showing how to create a new aircraft profile using the new panel selection method: https://vimeo.com/917361559

Video showing how to update existing aircraft profile to use the new panel selection method: https://vimeo.com/917364912 

* Added new virtual number pad to be used for touch enabled screen. This number pad will first focus the game window before sending num pad keystroke to the game.

* Added new feature to allow pop up panel as floating window. You can assign hotkeys (Ctrl-0 to Ctrl-9) to have the pop out to toggle either showing on screen or minimize.

Video showing how to manage floating panel: https://vimeo.com/918153200

* Added a new button to easily close all Pop Out Panel Manager's managed pop outs.
 
* Updated keyboard shortcut feature in preference setting to allow usage of custom keyboard shortcut instead of predefined set of keyboard shortcuts.

* Fixed few reported bugs in the application.

## Version 4.0.3

* Fixed rogue CPU issue when using touch feature in pop out panel which may cause computer to hang.

* Added a new turbo mode to improve execution speed during pop out process. Turbo mode can be turned on in preferences => general settings.

* Added support for PMDG 737 EFB.

* Added please wait message when editing panel source to resolve the confusion that Pop Out Panel Manager seems to hang when trying to select panel source during creation of a new aircraft profile.

* Improved touch reliability when touch feature on pop out panel.

* Removed pop out failure error message when after pop out predefined camera view has not been setup for an aircraft.

* Update text color for pop out progress dialog.

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

* Fixed logic where after panel source selection is completed, Pop Out Panel Manager will no longer recenter camera view. This is to remove confusion since Pop Out Manager has no way to tell the previous camera view you're on when panel source selection was started.

## Version 4.0.1.2
* Hotfix - Fixed issue where using touch panel feature may freeze computer and the application.

Known Issue:
If the profile has the option of "entire monitor display to have game refocus fucntion when touch" enabled, the configuration of display panels must be listed after the configuration of pop out panels in profile configuration.

## Version 4.0.1

* Added preference option in pop out settings to disable pop out progress messages from appearing.

* Added new feature to enable auto game refocus for entire display when using touch.

* Fixed issue when full screen mode is activated which may cause black bar to appear on either size (top/bottom or left/right).

* Fixed preferences pop out title bar color customization setting not saving.

* Fixed few UI issues from v4.0.

## Version 4.0.0

* Major improvement on responsiveness for touch and drag when using touch panel feature. (Please set Touch Down Touch Up Delay to 0 in preference setting to get the fastest performance)

* Brand new redesigned user interface. User will have the ability to edit panel configurations before panels are popped out.

* Added ability to add, remove, edit pop out panel for a profile without the need to redo the entire profile.

* Added ability to configure source instrumentation pop out panel location more easily.

* Added UI feedback when panels are being popped out. Panel configurations are being highlighted as they pop out. Also added onscreen status showing pop out progress.

* Added ability to navigate between profile and search for profile by name.

* Added ability to update profile name.

* Added ability to reorder panel pop out sequence. This may help resolve MSFS issue where panel may not pop out correctly unless they are popped out in certain sequence.

* Updated ability to use keyboard commands to move and adjust pop out panel's size and location without the need to click +/- pixel icons.

* Added auto game refocus option for non-touch enabled panel. An example use case is if you accidentally hover or click a non-touch enabled panel (PFD screen), now the flight control will not be locked out.

* Added preference option to automatically pause the game when pop out is in progress. A example use case is the game can pause during the start of landing challenge until pop out process is completed.

* Added preference option to move panel around when profile is locked. Panel configuration will still be locked and will not get changed.

* Added preference option to allow Pop Out Panel Manager to stay open during pop out process. 

* Added preference option to allow setting of pop out title bar color. Instead of the white title bar, the color can be set to blend in with Air Manager's background.

* Added preference option to delay auto pop out after Ready to Fly button is clicked by POPM plugin. For slower computer, this may resolve failed auto pop out because this process was executing too quickly after Ready to Fly button is clicked.

* Added preference option to disable automatic check for application update (for users who want to make sure application is not bypassing firewall and for privacy reason).

* Added a bonus HUD bar feature for PMDG 737 (which I fly the most) and generic aircraft. This includes a stop watch and adjust sim rate function. 

* Fixed an outstanding issue where panel configuration information is not accurate (top, left, width, height). Even though application was still working correctly in previous version, the configuration data is not quite correct. In this release, these panel configuration information are mostly fixed.

* Fixed an issue when applying hide title bar to panel, the location and size for the panel will be different than specified (off by about 9 pixels). With this fix, you will need to re-adjust the panel size for the hidden title bar panel.

* Updated user profile and application setting data file format. This new file format is not backward compatible. During first launch of the application, data migration will automatically take place. Backup files will also be created in the event you need to go back to previous version of POPM.

* Updated core code base to improve general application performance and resolve some outstanding bugs and issues regarding latest MSFS code SU12/AAU2.

* Updated application to use latest SimConnect SDK and .NET Framework 7.0.
 
* Added application rollback feature to restore previous version of the application (v3.4.6.0321) and restore backup of user profile and settings file.

Known issues:

* With rework of core panel configuration data structures, existing profile pop out panel placement may be off by a few pixels. Please use panel configuration function to adjust existing panel size and location if needed. This will only a one time ajustment per profile.

* When applying hide title bar to panel, even though the panel will now correctly fit into bezel or monitor you specified, the aspect ratio of the inner display will mostly be incorrect. This is an MSFS issue and I'm still investigating a workaround for this problem.


## Version 3.4.6.0321
* Added SU12 compatibility. This version is not backward compatible with previous MSFS release.


## Version 3.4.5
* Added new preference option by default to auto close MSFS Pop Out Manager when MSFS exits.

* Fixed an issue when using "Power on required to pop out panels for cold start" for G1000 and G3000 equipped aircraft, the pop out process gets stuck during the final step after panels have been popped out but before battery and avionics are to be turned off.


## Version 3.4.4.1011
* Hot fix: Reverted to previous implementation of touch setting's mouse cursor automatic refocus to center of MSFS game screen instead to the upper left corner of the pop out panel where it is being touched.


## Version 3.4.4
* Updated pop out panels separation reliability on all monitor resolutions during panel pop out process. A new algorithm had been implemented to improve Pop Out Panel Manager accuracy when it tries to click on panel's "magnifying glass" icon to separate panels.

* Added flight control refocus support for RealSimGear GTN750 Generation 1 when using touch enabled feature.

* Increased configurable maximum flight control refocus delay from 2 seconds to 10 seconds.
 

## Version 3.4.3
* Added ability to remember MSFS game window size and location for aircraft profile when running the game in windows display mode. This new preference setting is used to resize game window to match original size and location of MSFS game window when panel profile was defined initially. For existing aircraft profile, when running the game in windows display mode, the profile will automatically save MSFS game window position after the first successful pop out.

* Added ability to include in-game menu bar panels such as VFR Map, ATC, Checklist, etc to aircraft profile. During the pop out process, if any in-game menu bar panels are in popped out state, they will be included in panel configurations. This feature will only work if in-game menu bar panels are popped out if using in conjunction with auto pop out, it relies on MSFS re-opens these panels when flight starts (SU 10+). This also allows in-game menu bar panels to be touch enabled. Toggling the include in-game menu bar panels checkbox for a profile will reset these panels' inclusion and configurations. When using this feature with Auto Pop Out, there will be a delay in the pop out process to allow in-game menu bar panels to appear on screen before they're being configured to previously defined settings.

* Added UI cue to show number circle momentarily when popping out panel.

* Fixed an issue where touch does not work for panel when using full screen mode.


## Version 3.4.2
* Major change in how profile is bound to an aircraft. Previously, a profile is bound to an aircraft livery which requires you to activate binding when switching livery for the same aircraft. With this update, a profile is now bound to an aircraft so you no longer need to perform the binding step when switching livery. As you change active aircraft to fly, all existing livery binding will be automatically converted to aircraft binding if one exists. Also, a profile can still be bound to multiple aircrafts if you so choose such as when flying multiple variations of Cessna 172. This change has been a long awaited request to simplify your profile bindings. 

* Added auto assignment of aircraft binding for a newly created profile if the active aircraft has no previous profile binding specified.

* Added new keyboard shortcuts to move and resize pop outs. Please click on a new information icon in the upper right corner of panel configuration screen for instruction in how to use these new keyboard shortcuts.

* Added new setting to minimize pop out manager after panels have been popped out.

* Added work around for SU10 Beta issue when after panel separations, panels' size become so big and they block most of the game window for lower resolution screen and prevented Pop Out Panel Manager from popping out the next panel. 

* Made improvements to how panels are separated during pop out process.

* Fixed an issue when adjusting position and size of a pop out panel on some PC configuration.

Known issue:

* When changing the width or height of a pop up that has Hide Title Bar enable, it will sometime break the Hide Title Bar setting and the only way to fix this is to re-pop out the panel. Currently, this is a bug in MSFS in how it handles the sizing and rendering of pop outs window.


## Version 3.4.1

This release is solely focused on addressing issues regarding touch panel capabilities as well as making improvements to touch feature. Panels I used for testing are
PMT GTN750, WT G3X mod, PMS GTN530, FBW A32NX EFB, King Air 350 PFD/MFD touch screen.

When using SpaceDesk, please increase touch down touch up delay to 25ms in preference touch settings to improve sensitivity for touch input.

* Implemented new algorithm to improve general performance for touch panel.

* Button touches are now more responsive. On touch monitor, lag after touch had been minimized and is now performing much closer to a mouse click. On SpaceDesk, the lag is more in line with latency of remote display technology. When using SpaceDesk, you can increase touch delay to 25ms in preference settings to account for the latency if your touch is not register consistently.

* You can now slide your finger to pan map in panel. There is still a slight delay but touch response is much improved.

* Full screen mode for touch panel can now be turned on.

* Improved scroll bars touch response. They can be dragged and moved much more easily.

* Added adjustable flight control refocus (0.5 sec to 2 sec). The lowest setting makes flight control input refocus seems instantaneous after touch inactivity.

* Added ability to disable flight control refocus for a panel. Set panel to "Disable Game Refocus" will be useful in panel such as Flybywire A32NX EFB since textbox entries in this panel will lose focus if mouse cursor moves away from EFB. 


## Version 3.4

* Changed where user data files are stored. Previously, the files are saved in subfolder  "userdata" in your installation directory. Now they're moved to your Windows "Documents"  folder under "MSFS Pop Out Panel Manager" for easy access. When you first start the application, a data migration step will occur and your user data files will be upgraded and moved to this new folder location. This change will allow you to install Pop Out Manager to folder of your choice on your machine since the application no longer requires write access to your installation folder.

* Upgraded the application to use latest .NET 6.0 framework. You'll notice after user data migration, your installation folder will have only one executable file left.  Please don't be alarmed. This file is much bigger (67MB) since all .NET dependencies and application files are now packaged into a single file. You no longer need to manually install .NET framework to run this application.

* Major improvement in how Auto Pop Out Panel works. The app no longer tries to hunt and click "Ready to Fly" button by using "Ready to Fly Button Skipper" plugin I created. This plugin is included in the installation folder and is required for Auto Pop Out Panel to work. Please copy the folder "zzz-ready-to-fly-button-skipper" in subfolder "community" in your installation location into your MSFS community folder. [(Issue #29)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/29).

* Major improvement in support for touch enabled panel. Using SU10 Beta v1.27.11, touch panel capability has greatly improved including new support for touch down and touch up events. This update enables smooth touch operations (click and drag) for panel running on connected touch monitor or on tablet using software tool such as SpaceDesk. There is a new preference settings section for touch operation adjustments. Please see README.md for current known issue and workaround for touch operations.

* Updated how the application detects Sim Start and Sim Stop to perform Auto Pop Out. It now uses the much more reliable camera state SimConnect variable for detection.

* Fixed an issue where full screen mode panel does not expand the panel to fill the entire monitor. To use full screen mode correctly, once panel is popped out, move panel to desire monitor and adjust panel window to remove all top/bottom/left/right black bar. Then click on full screen mode checkbox for the panel. Next time when pop out is executed,  panel will be moved to correct monitor and it will expand to fill the entire screen.  [(Issue #27)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/27)

* Added a new feature to enable or disable fall back camera once pop out is completed. You can also define a custom camera view to load if centering cockpit view does not work for you. [(Issue #28)](https://github.com/hawkeye-stan/msfs-popout-panel-manager/issues/28)

* Added feature to change message dialog on screen duration. You can disable on screen message by setting the duration value to zero.

* (New for SU10+) Added additional keystroke option to pop out panel Ctrl + Right Ctrl + Left click instead of just Right-Alt +Left click. This is designed for users that have a keyboard without the Right-Alt key. To use this feature, please map (CTRL + RIGHT CTRL) in Control Options => Miscellaneous => New UI Window Mode in the game.

* Removed support of in game toolbar panels such as check list, ATC, or VFR map because something has changed in how Asobo coded these pop out windows. Unless Asobo fix/unfix the issue, this feature will not work correctly at all across both SU9 and SU10 beta. I will try to figure out a way to make this feature work again in feature version of the application.

* Cleaned up UI and improved UI guidance for user by enabling/disabling buttons and actions throughout the pop out process.

* Lots of the code is rewritten from the ground up for better performance and stability. It also prepares the code architecture for v4.0 new features. Please be patient as I continue to implement v4.0. As always, please report issue and comment and I welcome all feedbacks and will do my best to fix or implement requested features.

Known Issues:

* In SU 10 beta (1.27.11.0), when using Touch Enabled and Full Screen Mode simultaneously, touch event will not register correctly. Please run the panel in regular pop out window with "Hide toolbar" option instead of full screen mode.


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