This mod's purpose is to make content modder's lives easier by providing a set of very moddable classes and behaviours, with focus on flexibility.
Please do offer suggestions on what your needs are, especially things that could make it easier to implement hardcoded vanilla stuff.
Please check the [example mod](https://mods.vintagestory.at/show/mod/14708) for usage. I won't write documentation for now, so an example mod is a great solution because it is what I use for testing, allowing me to use it both to test and as an example for users.

Currently available classes:
- [Block Behaviour] `BreakSpawner`: Spawn entities when broken, allows to configure multiple entity codes, set their relative chances, or a required tool for the effect to be in place (allows wildcards selectors). Check example mod for usage.
- [Block Behaviour] `InfestedBlock`: Inherits from `BreakSpawner`, resembles minecraft's infested rock by breaking the block when a connected infested block is broken. Allows wildcards selectors for what nearby infested blocks will trigger the effect.
