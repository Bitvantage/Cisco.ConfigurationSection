/*
   Bitvantage.Cisco.ConfigurationSection
   Copyright (C) 2025 Michael Crino

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.

   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
