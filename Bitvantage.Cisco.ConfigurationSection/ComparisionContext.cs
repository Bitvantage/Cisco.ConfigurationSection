using System.Text;

namespace Bitvantage.Cisco
{
    public record ComparisionContext
    {
        public ComparisionContext(ConfigurationSection Section, Dictionary<ConfigurationSection, SectionMembership> SectionMatch)
        {
            this.Section = Section;
            this.SectionMatch = SectionMatch;
        }

        public ConfigurationSection Section { get; init; }
        public Dictionary<ConfigurationSection, SectionMembership> SectionMatch { get; init; }


        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            foreach (var section in Section.Descendants())
            {
                var type = SectionMatch[section];
                var linePrefix = type switch
                {
                    SectionMembership.First => '<',
                    SectionMembership.Second => '>',
                    SectionMembership.First | SectionMembership.Second => ' ',
                    _ => throw new ArgumentOutOfRangeException()
                };

                var stringReader = new StringReader(section.Line!);

                string? line;
                while ((line = stringReader.ReadLine()) != null)
                    stringBuilder.AppendLine($"{linePrefix} {new string(' ', section.Depth - 1)}{line}");
            }


            return stringBuilder.ToString();
        }
    }

}
