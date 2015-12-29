-- AntiCombatLog --

A Rocket plug-in that punishes players who disconnect from the server after a configurable number of seconds after taking damage.

```
<CombatLogGracePeriod>15</CombatLogGracePeriod> - How many seconds after getting hit do you have to wait to not be considered combat logging.
 
<WarningMessageColor>red</WarningMessageColor> - The color of all the messages the plug-in prints.

<ShowCombatLogMessagesToGlobalChat>true</ShowCombatLogMessagesToGlobalChat> - Tell all players on the server about the combat logger.

<ReminderNotifications>true</ReminderNotifications> - Remind the player of how many seconds are left until they can safely log out.

<ReminderNotificationInterval>5</ReminderNotificationInterval> - How many seconds apart to show the reminder notifications.

```

-- LIMITATIONS --

Since the list of players who have combat logged is stored in memory the player will not be informed that they have been punished if the server
restarts before they log back on.
