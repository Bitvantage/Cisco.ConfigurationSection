# TODO
* The additional indentations need to be a property of the parent node and not of each of the children
* Need better helper text on the root object than '\<Root\>'
* Option to remove or perhaps not remove '!' and '#' from the configuration section. Perhaps an options class or some sort of parse flags
* When removing a node, return the node's parent
* Make exceptions serializable
* Have some sort of specialized segment type that can hold commands with values like ranges and passwords
    * Would require a section factory
    * The range things still gets a bit awkward as sometimes ranges are multiline... but still it might be fine
    * Might also be interesting to be able to parse out things like interface configurations, but of course, there are lots of different formats... and would want to support some sort of update method?
* Have some sort of collection of fixups that the user can add stuff to. Call them all on Parse and ToString
* Might be useful to have packages of fixups and somehow bundle them so that the user could specify their platform and code and have them installed automatically
* Would be great to somehow extract the command syntax automatically by just fuzzing the parser
