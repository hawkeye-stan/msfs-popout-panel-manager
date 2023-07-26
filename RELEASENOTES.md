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

