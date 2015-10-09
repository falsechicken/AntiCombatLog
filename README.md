-- AntiCombatLog --

A Rocket plug-in that punishes players who disconnect from the server after a configurable number of seconds of taking damage.

```
<CombatLogGracePeriod>15</CombatLogGracePeriod> - How many seconds after getting hit do you have to wait to not be considered combat logging.
 
<WarningMessageColor>red</WarningMessageColor> - The color of all the messages the plug-in prints.

<ShowCombatLogMessagesToGlobalChat>true</ShowCombatLogMessagesToGlobalChat> - Tell all players on the server about the combat logger.
```

-- LIMITATIONS --

Currently combat loggers will not be punished if the server restarts before they log back on after combat logging. This is because the list of
combat loggers is stored in memory. Looking into a solution.
