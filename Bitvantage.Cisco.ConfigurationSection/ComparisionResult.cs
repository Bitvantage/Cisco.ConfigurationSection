namespace Bitvantage.Cisco
{
    public class ComparisionResult
    {
        internal ComparisionResult(ComparisionContext merged)
        {
            Merged = merged;
        }

        /// <summary>
        /// The union of the First and Second <see cref="ConfigurationSection"/>. Contains every <see cref="ConfigurationSection"/> from both the First and Second <see cref="ConfigurationSection"/>.
        /// </summary>
        public ComparisionContext Merged { get; }

        /// <summary>
        /// <see cref="ConfigurationSection"/> that are unique to the First <see cref="ConfigurationSection"/>
        /// </summary>
        public ComparisionContext UniqueToFirst()
        {
            return GenerateDeltaSection(SectionMembership.First);
        }

        /// <summary>
        /// <see cref="ConfigurationSection"/> that are unique to the Second <see cref="ConfigurationSection"/>
        /// </summary>
        public ComparisionContext UniqueToSecond()
        {
            return GenerateDeltaSection(SectionMembership.Second);
        }

        /// <summary>
        /// The intersection of <see cref="ConfigurationSection"/> that are common to both the First <see cref="ConfigurationSection"/> and the Second <see cref="ConfigurationSection"/>
        /// </summary>
        public ComparisionContext CommonToBoth()
        {
            return GenerateDeltaSection(SectionMembership.First | SectionMembership.Second);
        }

        /// <summary>
        /// Differences between the First <see cref="ConfigurationSection"/> and the Second <see cref="ConfigurationSection"/>
        /// </summary>
        public ComparisionContext Differences()
        {
            return GenerateDeltaSection(SectionMembership.First, SectionMembership.Second);
        }

        /// <summary>
        /// Generates a <see cref="ConfigurationSection"/> that transforms the First <see cref="ConfigurationSection"/> to the Second <see cref="ConfigurationSection"/>.
        /// Due to various inconsistencies in the configuration file structure the generated <see cref="ConfigurationSection"/> may not actually work. 
        /// </summary>
        public ComparisionContext Patch()
        {
            // BUG: if the line already starts with 'no' then what...
            
            // select sections that are unique to the first ConfigurationSection
            // reverse the order
            var sections = Merged
                .Section
                .Descendants()
                .Where(item => Merged.SectionMatch[item] == SectionMembership.First)
                .Reverse();

            var result = new ConfigurationSection();
            var matchTypes = new Dictionary<ConfigurationSection, SectionMembership>();
            var sectionHistory = new Dictionary<ConfigurationSection, ConfigurationSection>();
            foreach (var section in sections)
            {
                // ensure that the context is correct by rendering parent nodes
                var parent = result;
                foreach (var ancestor in section.Ancestors())
                {
                    if (!sectionHistory.TryGetValue(ancestor, out var historicParent))
                    {
                        parent = parent.Add(ancestor.Line!);
                        sectionHistory.Add(ancestor, parent);

                        if (matchTypes.TryGetValue(parent, out var value))
                            matchTypes[parent] = SectionMembership.First | value;
                        else
                            matchTypes[parent] = SectionMembership.First;
                    }
                    else
                        parent = historicParent;

                }

                ConfigurationSection sectionToRemove;
                if (section.Command.StartsWith("no "))
                    sectionToRemove = parent.Add($"{section.Command[3..]}");
                else
                    sectionToRemove = parent.Add($"no {section.Command}");

                matchTypes.Add(sectionToRemove,SectionMembership.First);
            }

            // add sections that are unique to the second ConfigurationSection
            sectionHistory.Clear();
            sections = Merged
                .Section
                .Descendants()
                .Where(item => Merged.SectionMatch[item] == SectionMembership.Second);

            foreach (var section in sections)
            {
                // ensure that the context is correct by rendering parent nodes
                var parent = result;
                foreach (var ancestor in section.AncestorsAndSelf())
                {
                    if (!sectionHistory.TryGetValue(ancestor, out var historicParent))
                    {
                        parent = parent.Add(ancestor.Line!);
                        sectionHistory.Add(ancestor, parent);

                        if (matchTypes.TryGetValue(parent, out var value))
                            matchTypes[parent] = SectionMembership.Second | value;
                        else
                            matchTypes[parent] = SectionMembership.Second;
                    }
                    else
                        parent = historicParent;

                }
            } 
            
            return new ComparisionContext(result, matchTypes);

        }
        private ComparisionContext GenerateDeltaSection(params SectionMembership[] mask)
        {
            // walk the merged ConfigurationSection

            var mergedSection = new ConfigurationSection();
            var matchTypes = new Dictionary<ConfigurationSection, SectionMembership>();

            // construct a new ConfigurationSection that contains the matched nodes and ancestors
            var sectionMapping = new Dictionary<ConfigurationSection, ConfigurationSection>();

            foreach (var sourceSection in Merged.Section.Descendants())
            {
                if (mask.Any(item => item == Merged.SectionMatch[sourceSection]))
                {
                    // add all ancestors and self to the results
                    ConfigurationSection? parent = mergedSection;
                    foreach (var section in sourceSection.AncestorsAndSelf())
                    {
                        if (!sectionMapping.TryGetValue(section, out var currentSection))
                        {
                            currentSection = parent.Add(section.Line!);
                            sectionMapping.Add(section, currentSection);
                            matchTypes.Add(currentSection, Merged.SectionMatch[section]);
                        }

                        parent = currentSection;
                    }
                }
            }

            return new ComparisionContext(mergedSection, matchTypes);
        }

    }

}
