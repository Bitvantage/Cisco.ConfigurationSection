# TODO
* The additional indentations need to be a property of the parent node and not of each of the children
* Need better helper text on the root object than '\<Root\>'
* Option to remove or perhaps not remove '!' and '#' from the configuration section. Perhaps an options class or some sort of parse flags
* When removing a node, return the nodes parent
* Make exceptions serilizable
* Have some sort of specialized segment type that can can hold commands with values like ranges and passwords
    * Would requie a section factory
    * The range things still gets a bit awackward as sometimes ranges are multiline... but still it might be fine
    * Might also be intersting to be able to parse out things like interface configurations, but of course there are lots of different formats... and would want to support some sort of update method?
* Have some sort of collection of fixups that the user can add stuff too. Call them all on Parse and ToString
* Might be useful to have packages of fixups and somehow bundle them so that the user could specificy their platform and code and have them installed automaticly
* Would be great to somehow extract the command syntax automaticly by just fuzzing the parser
