## Version 4.0.3

* Fixed rogue CPU issue when using touch feature in pop out panel which may cause computer to hang.

* Added a new turbo mode to improve execution speed during pop out process. Turbo mode can be turned on in preferences => general settings.

* Added support for PMDG 737 EFB.

* Added please wait message when editing panel source to resolve the confusion that Pop Out Panel Manager seems to hang when trying to select panel source during creation of a new aircraft profile.

* Improved touch reliability when touch feature on pop out panel.

* Removed pop out failure error message when after pop out predefined camera view has not been setup for an aircraft.

* Update text color for pop out progress dialog.

### KNOWN ISSUE: 

* For A2A Comanche PA-24-250, pop out step "Resetting camera view" does not work since this aircraft does not seem to implement SimConnect variable CAMERA_VIEW_TYPE_AND_INDEX:1 which Pop Out Panel Manager uses to reset camera view.
