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

using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Bitvantage.Cisco;

[DebuggerDisplay("{DebugHelper,nq}")]
public class ConfigurationSection : IList<ConfigurationSection>
{
    private static readonly Regex LineRegex = new(@"^(?<whiteSpaces>[ \t]*)(?<line>.*?)[ \t]*(?<newline>((\r?\n)|\Z))", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private readonly List<ConfigurationSection> _children = new();

    public int AdditionalIndention { get; }

    public ConfigurationSection BaseCommand
    {
        get
        {
            var baseCommand = this;
            while (baseCommand.Parent?.Parent != null) baseCommand = baseCommand.Parent;

            return baseCommand;
        }
    }

    public string Command
    {
        get
        {
            var stringReader = new StringReader(Line);

            return stringReader.ReadLine()!;
        }
    }

    private string DebugHelper
    {
        get
        {
            if (IsRoot)
                return "<Root>";

            return Command;
        }
    }

    public int Depth => AncestorsAndSelf().Count();

    public bool IsRoot => Parent == null;

    /// <summary>
    ///     Returns the first matching child node
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public ConfigurationSection this[string command] => this[command, StringComparison.Ordinal];

    /// <summary>
    ///     Returns the first matching child node
    /// </summary>
    /// <param name="command"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public ConfigurationSection this[string command, StringComparison comparisonType]
    {
        get
        {
            var section = _children.FirstOrDefault(item => item.Command.Equals(command, comparisonType));

            if (section == null)
                throw new KeyNotFoundException();

            return section;
        }
    }

    // TODO: should be Text or Value? It can be more than one line after all...
    // TODO: Should you be able to update this? I mean why not right?
    public string? Line { get; }

    public ConfigurationSection? Parent { get; internal set; }

    public string Path
    {
        get
        {
            if (IsRoot)
                return @"\";

            var sb = new StringBuilder();
            foreach (var section in AncestorsAndSelf())
            {
                sb.Append(@"\");
                sb.Append(section.Command);
            }

            return sb.ToString();
        }
    }


    // TODO: Why is this set? Why not just walk the tree?
    public ConfigurationSection? Root
    {
        get
        {
            var currentNode = this;
            while (currentNode.Parent != null)
                currentNode = currentNode.Parent;

            return currentNode;
        }
    }

    public ConfigurationSection()
    {
        Parent = null;
        Line = null;
    }

    internal ConfigurationSection(ConfigurationSection? parent, string? line, int additionalIndention)
    {
        Parent = parent;
        Line = line;
        AdditionalIndention = additionalIndention;
    }

    void ICollection<ConfigurationSection>.Add(ConfigurationSection item)
    {
        _children.Add(item);
    }

    public void Clear()
    {
        _children.Clear();
    }

    public bool Contains(ConfigurationSection item)
    {
        return _children.Contains(item);
    }

    public void CopyTo(ConfigurationSection[] array, int arrayIndex)
    {
        _children.CopyTo(array, arrayIndex);
    }

    public int Count => _children.Count;

    public bool IsReadOnly => ((ICollection<ConfigurationSection>)_children).IsReadOnly;

    bool ICollection<ConfigurationSection>.Remove(ConfigurationSection item)
    {
        return _children.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_children).GetEnumerator();
    }

    public IEnumerator<ConfigurationSection> GetEnumerator()
    {
        return _children.GetEnumerator();
    }

    public int IndexOf(ConfigurationSection item)
    {
        return _children.IndexOf(item);
    }

    void IList<ConfigurationSection>.Insert(int index, ConfigurationSection item)
    {
        _children.Insert(index, item);
    }

    public ConfigurationSection this[int index]
    {
        get => _children[index];
        set => _children[index] = value;
    }

    public void RemoveAt(int index)
    {
        _children.RemoveAt(index);
    }

    public void Add(ConfigurationSection section)
    {
        var sectionClone = section.Clone(this);

        if (Parent == null && _children.Count > 0 && _children.Last().Line == "end")
            _children.InsertRange(_children.Count - 1, sectionClone);
        else
            _children.AddRange(sectionClone);

    }

    public ConfigurationSection Add(string line)
    {
        return Add(line, 0);
    }

    public ConfigurationSection Add(string line, int additionalIndention)
    {
        var newSection = new ConfigurationSection(this, line, additionalIndention);

        if (Parent == null && _children.Count > 0 && _children.Last().Line == "end")
            _children.Insert(_children.Count - 1, newSection);
        else
            _children.Add(newSection);

        return newSection;
    }

    public ConfigurationSection AddAfterSelf(string line)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        return AddAfterSelf(line, 0);
    }

    public ConfigurationSection AddAfterSelf(string line, int additionalIndention)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var childIndex = Parent._children.IndexOf(this);

        return Parent.Insert(childIndex + 1, line, additionalIndention);
    }

    public void AddAfterSelf(ConfigurationSection section)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var childIndex = Parent._children.IndexOf(this);
        Parent.Insert(childIndex + 1, section);
    }

    public ConfigurationSection AddBeforeSelf(string line)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        return AddBeforeSelf(line, 0);
    }

    public ConfigurationSection AddBeforeSelf(string line, int additionalIndention)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var childIndex = Parent._children.IndexOf(this);

        return Parent.Insert(childIndex, line, additionalIndention);
    }

    public void AddBeforeSelf(ConfigurationSection section)
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var childIndex = Parent._children.IndexOf(this);
        Parent.Insert(childIndex, section);
    }

    public ConfigurationSection AddFirst(string line, int additionalIndention)
    {
        return Insert(0, line, additionalIndention);
    }

    public ConfigurationSection AddFirst(string line)
    {
        return AddFirst(line, 0);
    }

    public void AddFirst(ConfigurationSection section)
    {
        Insert(0, section);
    }

    public void AddRange(IEnumerable<ConfigurationSection> sections)
    {
        foreach (var section in sections)
        {
            var sectionClone = section.Clone(this);

            if (Parent == null && _children.Count > 0 && _children.Last().Line == "end")
                _children.InsertRange(_children.Count - 1, sectionClone);
            else
                _children.AddRange(sectionClone);
        }
    }

    public IEnumerable<ConfigurationSection> Ancestors()
    {
        //if (IsRoot)
        //    yield break;

        var stack = new Stack<ConfigurationSection>();
        var section = this;

        while (!section.IsRoot && !section.Parent!.IsRoot)
        {
            section = section.Parent;
            stack.Push(section);
        }

        while (stack.TryPop(out section))
            yield return section;
    }

    public IEnumerable<ConfigurationSection> AncestorsAndSelf()
    {
        if (IsRoot)
            return Enumerable.Empty<ConfigurationSection>();

        return Ancestors().Concat(new[] { this });
    }

    public JsonArray AsJson()
    {
        var rootNode = new JsonObject(new List<KeyValuePair<string, JsonNode?>> { new("Root", new JsonArray()) });
        var sectionQueue = new Queue<ConfigurationSection>();
        var nodeQueue = new Queue<JsonNode>();

        foreach (var child in Children())
        {
            sectionQueue.Enqueue(child);
            nodeQueue.Enqueue(rootNode);
        }

        while (sectionQueue.TryDequeue(out var currentSection))
        {
            var currentNode = nodeQueue.Dequeue();

            JsonNode newNode;
            if (currentSection.Children().Any())
                newNode = new JsonObject(new List<KeyValuePair<string, JsonNode?>> { new(currentSection.Line!, new JsonArray()) });
            else
                newNode = JsonValue.Create(currentSection.Line)!;

            currentNode.AsObject().Single().Value!.AsArray().Add(newNode);

            foreach (var child in currentSection.Children())
            {
                sectionQueue.Enqueue(child);
                nodeQueue.Enqueue(newNode);
            }
        }

        return rootNode.AsObject().Single().Value!.AsArray();
    }

    public IEnumerable<ConfigurationSection> Children()
    {
        return _children.AsEnumerable();
    }

    public IEnumerable<ConfigurationSection> ChildrenAndSelf()
    {
        return new[] { this }.Concat(_children);
    }

    public List<ConfigurationSection> Clone(ConfigurationSection newParent)
    {
        if (IsRoot)
            return CloneChildren(newParent);

        return new List<ConfigurationSection>(new []{CloneSelf(newParent)});
    }

    public List<ConfigurationSection> CloneChildren(ConfigurationSection parent)
    {
        // Create a stack to manage the sections to be cloned
        var stack = new Stack<(ConfigurationSection original, ConfigurationSection clone)>();

        // Create a new root
        // the root node's only purpose is to hold the cloned children
        var rootClone = new ConfigurationSection();

        foreach (var child in _children)
        {
            var childClone = new ConfigurationSection(parent, child.Line, child.AdditionalIndention);

            // Add the cloned child to the root
            rootClone._children.Add(childClone);

            // Push the child section onto the stack for further processing
            stack.Push((child, childClone));
        }

        while (stack.Count > 0)
        {
            // Pop a section from the stack
            var (currentOriginal, currentClone) = stack.Pop();

            // Clone each child of the current section
            foreach (var child in currentOriginal)
            {
                var childClone = new ConfigurationSection(currentClone, child.Line, child.AdditionalIndention);

                // Add the cloned child to the current clone
                currentClone._children.Add(childClone);

                // Push the child section onto the stack for further processing
                stack.Push((child, childClone));
            }
        }

        return rootClone._children;
    }

    public ConfigurationSection CloneSelf(ConfigurationSection parent)
    {
        // Create a stack to manage the sections to be cloned
        var stack = new Stack<(ConfigurationSection original, ConfigurationSection clone)>();

        // Create the root clone
        var rootClone = new ConfigurationSection(parent, Line, AdditionalIndention);
        stack.Push((this, rootClone));

        while (stack.Count > 0)
        {
            // Pop a section from the stack
            var (currentOriginal, currentClone) = stack.Pop();

            // Clone each child of the current section
            foreach (var child in currentOriginal)
            {
                var childClone = new ConfigurationSection(currentClone, child.Line, child.AdditionalIndention);

                // Add the cloned child to the current clone
                currentClone._children.Add(childClone);

                // Push the child section onto the stack for further processing
                stack.Push((child, childClone));
            }
        }

        return rootClone;
    }

    public ComparisionResults Compare(IEnumerable<ConfigurationSection> otherSections)
    {
        return Compare(Children(), otherSections);
    }

    public static ComparisionResults Compare(IEnumerable<ConfigurationSection> firstSections, IEnumerable<ConfigurationSection> secondSections)
    {
        var firstSectionList = firstSections.ToList();
        var secondSectionList = secondSections.ToList();

        var commonToBoth = firstSectionList
            .SelectMany(item => item.DescendantsAndSelf())
            .IntersectBy(
                secondSectionList
                    .SelectMany(item => item.DescendantsAndSelf())
                    .Select(item => item.Path), section => section.Path)
            .ToList();

        var uniqueToFirst = firstSectionList
            .SelectMany(item => item.DescendantsAndSelf())
            .ExceptBy(
                secondSectionList
                    .SelectMany(item => item.DescendantsAndSelf())
                    .Select(item => item.Path), section => section.Path)
            .ToList();

        var uniqueToSecond = secondSectionList
            .SelectMany(item => item.DescendantsAndSelf())
            .ExceptBy(
                firstSectionList
                    .SelectMany(item => item.DescendantsAndSelf())
                    .Select(item => item.Path), section => section.Path)
            .ToList();

        return new ComparisionResults(commonToBoth, uniqueToFirst, uniqueToSecond);
    }

    public bool Contains(params string[] command)
    {
        return Find(command).Any();
    }

    public IEnumerable<ConfigurationSection> Descendants()
    {
        var stack = new Stack<ConfigurationSection>();

        for (var childIndex = _children.Count - 1; childIndex >= 0; childIndex--)
            stack.Push(_children[childIndex]);

        while (stack.TryPop(out var section))
        {
            yield return section;

            for (var childIndex = section._children.Count - 1; childIndex >= 0; childIndex--)
                stack.Push(section._children[childIndex]);
        }
    }

    public IEnumerable<ConfigurationSection> DescendantsAndSelf()
    {
        return new[] { this }.Concat(Descendants());
    }

    /// <summary>
    ///     Matches a sequence of strings
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public ConfigurationSection[] Find(params string[] patterns)
    {
        var matches = new List<ConfigurationSection>();

        FindChildren(patterns, 0, _children, matches);

        return matches.ToArray();
    }

    private void FindChildren(string[] patterns, int startIndex, List<ConfigurationSection> sections, List<ConfigurationSection> matches)
    {
        foreach (var section in sections)
            if (patterns[startIndex] == section.Command)
            {
                if (startIndex == patterns.Length - 1)
                    matches.Add(section);
                else
                    FindChildren(patterns, startIndex + 1, section._children, matches);
            }
    }

    public ConfigurationSection GetOrAdd(string line)
    {
        return GetOrAdd(line, 0);
    }

    public ConfigurationSection GetOrAdd(string line, int additionalIndentation)
    {
        var section = Parent
            ._children
            .FirstOrDefault(item => item.Line == line);

        if (section != null)
            return section;

        return Add(line, additionalIndentation);
    }

    public ConfigurationSection Insert(int index, string line, int additionalIndention)
    {
        var newSection = new ConfigurationSection(this, line, additionalIndention);
        _children.Insert(index, newSection);

        return newSection;
    }

    public ConfigurationSection Insert(int index, string line)
    {
        return Insert(index, line, 0);
    }

    public void Insert(int index, ConfigurationSection section)
    {
        var sectionClone = section.Clone(this);
        _children.InsertRange(index, sectionClone);
    }

    /// <summary>
    ///     Matches a sequence of regular expressions
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public SectionMatchGroup[] Match([StringSyntax(StringSyntaxAttribute.Regex)] params string[] patterns)
    {
        var regex = patterns
            .Select(item => new Regex(item, RegexOptions.Compiled))
            .ToArray();

        var matches = SearchChildren(regex, _children);

        return matches;
    }

    /// <summary>
    ///     Matches a sequence of regular expressions
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public SectionMatchGroup[] Match(params Regex[] patterns)
    {
        var matches = SearchChildren(patterns, _children);
        return matches;
    }

    public ConfigurationSection Next()
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var index = Parent._children.IndexOf(this);

        if (index == Parent._children.Count - 1)
            throw new IndexOutOfRangeException();

        return Parent._children[index + 1];
    }

    public ConfigurationSection? NextOrDefault()
    {
        if (Parent == null)
            return null;

        var index = Parent._children.IndexOf(this);

        if (index == Parent._children.Count - 1)
            return null;

        return Parent._children[index + 1];
    }

    public static ConfigurationSection Parse(string configuration)
    {
        var root = new ConfigurationSection(null, null, 0);

        var lineMatches = LineRegex.Matches(configuration);

        var stack = new Stack<SectionText>();
        stack.Push(new SectionText(0, root));

        var previousNode = new SectionText(0, root);

        var configState = ConfigState.Start;
        var lineNumber = 0;

        var currentIndentionLevel = 0;
        var previousIndentionLevel = 0;

        StringBuilder currentBanner = null;
        string bannerEndDelimiter = null;

        string line;
        string newlineSequence = null;

        var additionalIndentations = 0;

        readNextLine:
        {
            if (lineNumber >= lineMatches.Count)
                goto end;

            var lineMatch = lineMatches[lineNumber++];
            line = lineMatch.Groups["line"].Value;
            currentIndentionLevel = lineMatch.Groups["whiteSpaces"].Value.Length;
            newlineSequence = lineMatch.Groups["newline"].Value;

            if (configState == ConfigState.Start && line.StartsWith("Building configuration..."))
            {
                configState = ConfigState.BuildingConfig;
                goto readNextLine;
            }

            if (configState is ConfigState.Start or ConfigState.BuildingConfig && line.StartsWith("Current configuration : "))
            {
                configState = ConfigState.Config;
                goto readNextLine;
            }

            if (configState == ConfigState.Banner)
                goto handleBanner;

            if (currentIndentionLevel == 0 && line.StartsWith("banner "))
            {
                configState = ConfigState.Banner;
                goto startBanner;
            }

            configState = ConfigState.Config;

            goto handleConfig;
        }

        startBanner:
        {
            var bannerDelimiterSection = line.Split(' ')[2];

            if (bannerDelimiterSection.StartsWith("^C"))
                bannerEndDelimiter = "^C";
            else
                bannerEndDelimiter = bannerDelimiterSection[0].ToString();

            currentBanner = new StringBuilder();
            currentBanner.Append(line);
            currentBanner.Append(newlineSequence);

            stack.Clear();
            stack.Push(new SectionText(0, root));
            previousIndentionLevel = 0;

            goto readNextLine;
        }

        handleBanner:
        {
            currentBanner.Append(new string(' ', currentIndentionLevel));
            currentBanner.Append(line);

            // if this line is the banner delimiter then save the complete banner as a single node 
            if (line.EndsWith(bannerEndDelimiter))
            {
                var bannerNode = new ConfigurationSection(root, currentBanner.ToString(), 0);

                previousNode = new SectionText(0, bannerNode);

                var parentSection = stack.Peek().Section;
                parentSection._children.Add(new ConfigurationSection(parentSection, currentBanner.ToString(), 0));
                configState = ConfigState.Config;
            }
            else
            {
                currentBanner.Append(newlineSequence);
            }

            goto readNextLine;
        }

        handleConfig:
        {
            // skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                goto readNextLine;

            // a child of the current node
            // add a new indention level
            if (currentIndentionLevel > previousIndentionLevel)
            {
                stack.Push(new SectionText(previousIndentionLevel, previousNode.Section));
                additionalIndentations = currentIndentionLevel - previousIndentionLevel - 1;
            }
            // a child of a parent node
            // revert to a previous indention level
            else if (currentIndentionLevel < previousIndentionLevel)
            {
                do
                {
                    previousNode = stack.Pop();
                } while (previousNode.IndentionLevel > currentIndentionLevel);
            }

            previousIndentionLevel = currentIndentionLevel;

            var parentSection = stack.Peek().Section;

            parentSection._children.Add(new ConfigurationSection(parentSection, line, additionalIndentations));

            var childNode = new SectionText(currentIndentionLevel, stack.Peek().Section.Children().Last());

            previousNode = childNode;

            if (currentIndentionLevel == 0 && line == "end")
            {
                configState = ConfigState.End;
                goto end;
            }

            goto readNextLine;
        }

        end:
        {
        }

        return root;
    }

    public ConfigurationSection Previous()
    {
        if (Parent == null)
            throw new IndexOutOfRangeException();

        var index = Parent._children.IndexOf(this);

        if (index == 0)
            throw new IndexOutOfRangeException();

        return Parent._children[index - 1];
    }

    public ConfigurationSection? PreviousOrDefault()
    {
        if (Parent == null)
            return null;

        var index = Parent._children.IndexOf(this);

        if (index == 0)
            return null;

        return Parent._children[index - 1];
    }

    public void Remove(ConfigurationSection section)
    {
        var success = _children.Remove(section);

        if (!success)
            throw new KeyNotFoundException();
    }

    public void Remove(string command)
    {
        var sections = _children
            .Where(item => item.Command == command)
            .ToList();

        if (sections.Count == 0)
            throw new KeyNotFoundException();

        foreach (var section in sections)
            section.RemoveSelf();
    }

    public void RemoveSelf()
    {
        Parent._children.Remove(this);
    }

    public ConfigurationSection Replace(ConfigurationSection section)
    {
        var childIndex = Parent._children.IndexOf(this);
        RemoveSelf();

        Parent.Insert(childIndex, section);

        return section;
    }

    public ConfigurationSection Replace(string line)
    {
        return Replace(line, 0, false);
    }

    public ConfigurationSection Replace(string line, bool keepChildren)
    {
        return Replace(line, 0, keepChildren);
    }

    public ConfigurationSection Replace(string line, int additionalIndention)
    {
        return Replace(line, additionalIndention, false);
    }

    public ConfigurationSection Replace(string line, int additionalIndention, bool keepChildren)
    {
        // get the index of this node
        var childIndex = Parent._children.IndexOf(this);

        // create a new node to replace this node
        var newSection = new ConfigurationSection(this, line, additionalIndention);
        
        // optionally copy children to the replacement node
        if (keepChildren)
            newSection.AddRange(this);

        // remove this node from its parent
        RemoveSelf();

        // insert the replacement node in this nodes place
        Parent._children.Insert(childIndex, newSection);

        return newSection;
    }

    /// <summary>
    ///     Matches a sequence of regular expressions
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public ConfigurationSection[] Search([StringSyntax(StringSyntaxAttribute.Regex)] params string[] patterns)
    {
        return Match(patterns).Select(item => item.Section).ToArray();
    }

    /// <summary>
    ///     Matches a sequence of regular expressions
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public ConfigurationSection[] Search(params Regex[] patterns)
    {
        return Match(patterns).Select(item => item.Section).ToArray();
    }

    public SectionMatchGroup[] SearchChildren(Regex[] patterns, List<ConfigurationSection> sections)
    {
        List<SectionMatchGroup> matches = new();

        var matchHistory = new Stack<SectionMatch>();

        // add children to the search stack
        var searchStack = new Stack<(ConfigurationSection Section, int Depth)>(sections.Select(section => (item: section, 0)).Reverse());

        while (searchStack.TryPop(out var current))
        {
            // remove dead history frames
            while (matchHistory.Count > current.Depth)
                matchHistory.Pop();

            var match = patterns[current.Depth].Match(current.Section.Command);
            if (match.Success)
            {
                matchHistory.Push(new SectionMatch(match, current.Section));

                // if the maximum depth has been reached, add the history to the match list
                if (current.Depth == patterns.Length - 1)
                    matches.Add(new SectionMatchGroup(matchHistory.Reverse().ToArray()));
                // otherwise push any children onto the stack
                else
                    foreach (var child in current.Section.Children().Reverse())
                        searchStack.Push((child, current.Depth + 1));
            }
        }

        return matches.ToArray();
    }

    public override string ToString()
    {
        var output = new StringBuilder();

        if (Parent != null)
            output.AppendLine(Line);

        var indentOffset = AdditionalIndention;
        foreach (var section in Descendants())
        {
            output.Append(new string(' ', GetIndentLevel(section)));
            output.AppendLine(section.Line);
        }

        return output.ToString();

        int GetIndentLevel(ConfigurationSection section)
        {
            var indentLevel = 0;
            var currentNode = section;

            while (currentNode != this && currentNode?.Parent?.Parent != null)
            {
                indentLevel = indentLevel + 1 + currentNode.AdditionalIndention;
                currentNode = currentNode.Parent;
            }

            return indentLevel;
        }
    }

    public bool TryGet(string command, [NotNullWhen(true)] out ConfigurationSection? section)
    {
        var foundSection = _children.FirstOrDefault(item => item.Command == command);

        if (foundSection == null)
        {
            section = null;
            return false;
        }

        section = foundSection;
        return true;
    }

    public bool TryRemove(ConfigurationSection section)
    {
        return _children.Remove(section);
    }

    public bool TryRemove(string command)
    {
        var sections = _children
            .Where(item => item.Command == command)
            .ToList();

        foreach (var section in sections)
            section.RemoveSelf();

        return sections.Any();
    }

    private enum ConfigState
    {
        Start,
        BuildingConfig,
        Config,
        Banner,
        End
    }

    private record SectionText(int IndentionLevel, ConfigurationSection Section)
    {
    }
}