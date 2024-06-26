/*
   Bitvantage.Cisco.ConfigurationSection
   Copyright (C) 2024 Michael Crino
   
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

namespace Bitvantage.Cisco;

public record ComparisionResults(List<ConfigurationSection> CommonToBoth, List<ConfigurationSection> UniqueToFirst, List<ConfigurationSection> UniqueToSecond)
{
    /// <summary>
    /// Generate a merge file that transforms the second configuration to the first configuration
    /// </summary>
    /// <returns></returns>
    public ConfigurationSection Merge()
    {
        var root = new ConfigurationSection();

        // first remove lines that are unique to the second configuration
        var uniqueToSecond = UniqueToSecond
            .GroupBy(item => item.Parent)
            .OrderByDescending(item => item.Key.Depth)
            .ToList();
        ConfigurationSection? last = root;

        foreach (var section in uniqueToSecond)
        {
            last = root;
            // output the parent commands
            foreach (var sectionX in section.Key.AncestorsAndSelf()) 
                last = last.Add(sectionX.Line, sectionX.AdditionalIndention);

            // negate each command
            foreach (var configurationSection in section)
            {
                // if the command already starts with a no, do the command without the no
                if (configurationSection.Line.StartsWith("no "))
                    last.Add(configurationSection.Line[3..], configurationSection.AdditionalIndention);
                else
                    last.Add($"no {configurationSection.Line}", configurationSection.AdditionalIndention);
            }

            root.Add("!");
        }

        // next add lines that are unique to the second configuration
        var uniqueToFirst = UniqueToFirst
            .SelectMany(item => item.AncestorsAndSelf())
            .Distinct()
            //.OrderBy(item => item.SequenceId)
            .ToList();

        var lastBaseCommand = uniqueToFirst.FirstOrDefault()?.BaseCommand;
        last = root.Root;

        foreach (var section in uniqueToFirst)
        {
            if (section.BaseCommand != lastBaseCommand)
            {
                root.Add("!");
                lastBaseCommand = section.BaseCommand;
                last = root.Root;
            }

            while (last.Depth >= section.Depth) 
                last = last.Parent;

            last = last.Add(section.Line, section.AdditionalIndention);
        }

        if (lastBaseCommand != null)
            root.Add("!");

        return root;
    }

}