## Version 4.0.3 Beta 2

* Update logic to turbo mode to help resolve pop out reliability issue.

* Added please wait message when editing panel source to resolve the confusion that Pop Out Panel Manager seems to hang when trying to select panel source when creating a new aircraft profile. 

* Remove pop out failure error message when after pop out predefined camera view has not been setup for an aircraft.

* KNOWN ISSUE: For A2A Comanche PA-24-250, pop out step "Resetting camera view" does not work since this aircraft does not seem to implement SimConnect variable CAMERA_VIEW_TYPE_AND_INDEX:1 which Pop Out Panel Manager uses.

## Version 4.0.3 Beta 1

* Added a new turbo mode to improve execution speed during pop out process. Turbo mode can be turned on in preferences => general settings.

* Update text color for pop out progress dialog.