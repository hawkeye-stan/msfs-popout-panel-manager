# Version History
<hr/>

## Version 3.1.0
* Updated UI to use menu bar to control most application settings.
* Added long awaited auto save feature. Application no longer requires user to manually save profile after each panel change. All panel adjustments are automatically save.
* Added panel lock feature to complement autosave feature. When panels are lock and are being moved or resize, their new location information will not get save. Also, for instrumentation pop out panels, when panels are locked, any accidental movement of the panels will return them to previously saved locations. For built-in panels such as ATC, VFR Map, etc, they can still be moved but their saved location will not get change.
* Added keyboard shortcuts for commonly used function. The buttons for -10px, -1px, +1px, and +10px now has keyboard shortcut of 'Ctrl -', 'Ctrl [', 'Ctrl ]', and 'Ctrl +' respectively.
* Added miminize all panels feature. This allows user to temporary see what is behind all open pop out panels. (This is an user requested feature with Github ticket #6).
* Various smaller bug fixes and usability enhancements.
 


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