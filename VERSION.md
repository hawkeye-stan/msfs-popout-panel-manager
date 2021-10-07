# Version History
<hr/>

## Vesion 2.0.2.0
* Added one second delay on mouse click when the application is trying to separate the chained pop out windows.

## Vesion 2.0.1.0
* Changed how screen resolution is detected. Used vertical instead of horizontal resolution to account for ultra wide monitors.

## Version 2.0.0.0
* Used new image recognition instead of OCR technology to determine pop outs.
* Added auto pop out feature.
* Allowed moving pop out panels using coordinates/width/height after analysis.
* Added additional plane profiles.
* Running on non-native monitor resolution will not work because of image scaling issue when doing image analysis.

## Version 1.2.0.0
* Increase OCR image accuracy by raising image DPI before analysis.
* Added (very experimental) Asobo A320 and FlybyWire A320NX profiles as testing sample. These profiles do only work 100% of the time. Continue investigation into better OCR accuracy will be needed.
* Added profile dropdown sorted by profile name.
* Fixed an issue of reapplying the same settings will cause panels to be out of sync.
* Fixed an issue of switching profiles will cause panels to be out of sync.
* Fixed an issue of unable to set or reset panel to NOT ALWAYS ON TOP.
* Fixed application path issue for not able to find ocrdata.json file at startup.
* Removed MSFS Pop Out Panel Manager is always on top. This is intefering with image operations. 

## Version 1.1.0.0
* Added caption title for the "untitled" windows. After analysis, if the panel window matches the name in the profile/ocr definition file, it will now display a caption of "Custom - XXXXX" (ie. Custom - PFD). This allows user to use various 3rd party windows layout manager to organize pop out panel windows.
* Added hide panel title bar feature.
* Added ability to have pop out panels to be always on top.
* Added minimize application to tray feature.
* Made application flow more intuitive.
* Fixed various small bugs in the application.

## Version 1.0.0.0
* Initial Release