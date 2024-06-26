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

using System.Net;
using Bitvantage.Cisco;

namespace Test;

public class Tests
{
    [Test]
    public void Add01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        configuration
            .Add("item 6")
            .Add("item 6.1").Parent!
            .Add("item 6.1.1").Parent!.Parent!
            .Add("item 6.2").Parent!
            .Add("item 6.3");

        Assert.That(configuration.ToString(), Is.EqualTo("""
            item 1
             item 1.1
              item 1.1.1
              item 1.1.2
              item 1.1.3
             item 1.2
             item 1.3
              item 1.3.1
              item 1.3.2
             item 1.4
             item 1.5
            banner mod ^
            text
             text
              text
               text
                 text
            ^
            item 2
            item 3
             item 3.1
             item 3.2
            item 4
            item 5
             item 5.1
                item 5.2
                item 5.3
                item 5.4
                item 5.5
            item 6
             item 6.1
             item 6.1.1
            item 6.2
            item 6.3
            end

            """));
    }

    [Test]
    public void Add02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var newSection = new ConfigurationSection();
        newSection.Add("xitem 2");
        newSection.Add("xitem 3")
            .Add("xitem 3.4");

        configuration.Add(newSection);

        Assert.That(
            configuration.ToString(),
            Is.EqualTo("""
                item 1
                 item 1.1
                  item 1.1.1
                  item 1.1.2
                  item 1.1.3
                 item 1.2
                 item 1.3
                  item 1.3.1
                  item 1.3.2
                 item 1.4
                 item 1.5
                banner mod ^
                text
                 text
                  text
                   text
                     text
                ^
                item 2
                item 3
                 item 3.1
                 item 3.2
                item 4
                item 5
                 item 5.1
                    item 5.2
                    item 5.3
                    item 5.4
                    item 5.5
                xitem 2
                xitem 3
                 xitem 3.4
                end

                """));
    }

    [Test]
    public void Add03()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var newSection = new ConfigurationSection();
        newSection.Add("xitem 2");
        newSection.Add("xitem 3")
            .Add("xitem 3.4");

        configuration["item 5"].Add(newSection);

        Assert.That(
            configuration.ToString(),
            Is.EqualTo("""
                item 1
                 item 1.1
                  item 1.1.1
                  item 1.1.2
                  item 1.1.3
                 item 1.2
                 item 1.3
                  item 1.3.1
                  item 1.3.2
                 item 1.4
                 item 1.5
                banner mod ^
                text
                 text
                  text
                   text
                     text
                ^
                item 2
                item 3
                 item 3.1
                 item 3.2
                item 4
                item 5
                 item 5.1
                    item 5.2
                    item 5.3
                    item 5.4
                    item 5.5
                 xitem 2
                 xitem 3
                  xitem 3.4
                end

                """));
    }

    [Test]
    public void AddAfterSelf01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"]["Line B.B"].AddAfterSelf("Test 1");
        configuration["Line D"].AddAfterSelf("Test 2");
        configuration["Line D"]["Line D.A"].AddAfterSelf("Test 3");
        configuration["Line D"]["Line D.A"]["Line D.A.A"].AddAfterSelf("Test 4");

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line B.B
              Line B.A.A
             Test 1
            Line D
             Line D.A
              Line D.A.A
              Test 4
             Test 3
            Test 2

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddAfterSelf02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"].AddAfterSelf(ConfigurationSection.Parse(TestData.ConfigurationFragment05));

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line B.B
              Line B.A.A
            Line E
             Line E.A
              Line E.A.B
            Line F
             Line F.B
              Line F.A.A
            Line G
             Line G.A
              Line G.A.A
            Line D
             Line D.A
              Line D.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddBeforeSelf01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"]["Line B.B"].AddBeforeSelf("Test 1");
        configuration["Line D"].AddBeforeSelf("Test 2");
        configuration["Line D"]["Line D.A"].AddBeforeSelf("Test 3");
        configuration["Line D"]["Line D.A"]["Line D.A.A"].AddBeforeSelf("Test 4");

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Test 1
             Line B.B
              Line B.A.A
            Test 2
            Line D
             Test 3
             Line D.A
              Test 4
              Line D.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddBeforeSelf02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"].AddBeforeSelf(ConfigurationSection.Parse(TestData.ConfigurationFragment05));

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line E
             Line E.A
              Line E.A.B
            Line F
             Line F.B
              Line F.A.A
            Line G
             Line G.A
              Line G.A.A
            Line B
             Line B.B
              Line B.A.A
            Line D
             Line D.A
              Line D.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddFirst01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"]["Line B.B"].AddFirst("Test 1");
        configuration["Line D"].AddFirst("Test 2");
        configuration["Line D"]["Line D.A"].AddFirst("Test 3");
        configuration["Line D"]["Line D.A"]["Line D.A.A"].AddFirst("Test 4");

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line B.B
              Test 1
              Line B.A.A
            Line D
             Test 2
             Line D.A
              Test 3
              Line D.A.A
               Test 4

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddFirst02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"].AddFirst(ConfigurationSection.Parse(TestData.ConfigurationFragment05));

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line E
              Line E.A
               Line E.A.B
             Line F
              Line F.B
               Line F.A.A
             Line G
              Line G.A
               Line G.A.A
             Line B.B
              Line B.A.A
            Line D
             Line D.A
              Line D.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Insert01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment03);

        configuration["Line B"]["Line B.A"].Insert(1, ConfigurationSection.Parse(TestData.ConfigurationFragment05));

        var expected = """
            Line A
             Line A.A
              Line A.A.A
            Line B
             Line B.A
              Line B.A.A
              Line E
               Line E.A
                Line E.A.B
              Line F
               Line F.B
                Line F.A.A
              Line G
               Line G.A
                Line G.A.A
              Line B.A.B
              Line B.A.C
             Line B.B
              Line B.B.A
              Line B.B.B
            Line C
             Line C.A
              Line C.A.A
            Line E
             Line E.A
              Line E.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void AddRange01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var newSection = new ConfigurationSection();
        newSection.Add("xitem 2");
        newSection.Add("xitem 3")
            .Add("xitem 3.4");

        configuration.AddRange(newSection.Children());

        Assert.That(
            configuration.ToString(),
            Is.EqualTo("""
                item 1
                 item 1.1
                  item 1.1.1
                  item 1.1.2
                  item 1.1.3
                 item 1.2
                 item 1.3
                  item 1.3.1
                  item 1.3.2
                 item 1.4
                 item 1.5
                banner mod ^
                text
                 text
                  text
                   text
                     text
                ^
                item 2
                item 3
                 item 3.1
                 item 3.2
                item 4
                item 5
                 item 5.1
                    item 5.2
                    item 5.3
                    item 5.4
                    item 5.5
                xitem 2
                xitem 3
                 xitem 3.4
                end

                """));
    }

    [Test]
    public void Ancestors01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.That(configuration[0].Ancestors(), Is.Empty);
        Assert.That(configuration[0].AncestorsAndSelf(), Is.EqualTo(new[] { configuration[0] }));

        Assert.That(configuration[0][0].Ancestors(), Is.EqualTo(new[] { configuration[0] }));
        Assert.That(configuration[0][0].AncestorsAndSelf(), Is.EqualTo(new[] { configuration[0], configuration[0][0] }));

        Assert.That(configuration[0][0][0].Ancestors(), Is.EqualTo(new[] { configuration[0], configuration[0][0] }));
        Assert.That(configuration[0][0][0].AncestorsAndSelf(), Is.EqualTo(new[] { configuration[0], configuration[0][0], configuration[0][0][0] }));
    }

    [Test]
    public void Banner01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment02);

        Assert.That(configuration.Children().Count(), Is.EqualTo(4));

        Assert.That(
            configuration[0].Line,
            Is.EqualTo(
                """
                banner test1 ^
                line 1
                line 2
                 line 3^
                """));

        Assert.That(
            configuration[1].Line,
            Is.EqualTo(
                """
                banner test2 X
                line 1
                line 2X
                """));

        Assert.That(
            configuration[2].Line,
            Is.EqualTo(
                """
                banner test3 X
                line 1
                line 2
                X
                """));


        Assert.That(
            configuration[3].Line,
            Is.EqualTo(
                """
                banner test4 ^Cfasfasfasdf
                line 1
                ^C
                """));
    }

    [Test]
    public void Banner02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment02);

        Assert.That(configuration[0].Path, Is.EqualTo(@"\banner test1 ^"));
    }

    [Test]
    public void BaseCommand01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.That(configuration[0][0][2].BaseCommand.Line, Is.EqualTo("item 1"));
    }

    [Test]
    public void ChildrenAndSelf01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        var resultLines = configuration
            .ChildrenAndSelf()
            .Select(item => item.ToString())
            .ToList();

        var results = string.Concat(resultLines);

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line B.B
              Line B.A.A
            Line D
             Line D.A
              Line D.A.A
            Line A
             Line A.A
              Line A.A.B
            Line B
             Line B.B
              Line B.A.A
            Line D
             Line D.A
              Line D.A.A

            """;

        Assert.That(resultLines.Count, Is.EqualTo(4));
        Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void Command01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment02);

        Assert.That(configuration[0].Command, Is.EqualTo("banner test1 ^"));
    }

    [Test]
    public void Compare01()
    {
        var left = ConfigurationSection.Parse(TestData.ConfigurationFragment03);
        var right = ConfigurationSection.Parse(TestData.ConfigurationFragment04);
        //var left = new Section()
        //    .Add("Line A")
        //        .Add("Line A.A")
        //            .Add("Line A.A.A").Root
        //    .Add("Line B")
        //        .Add("Line B.A")
        //            .Add("Line B.A.A").Root
        //    .Add("Line C")
        //        .Add("Line C.A")
        //            .Add("Line C.A.A").Root;

        //var right = new Section()
        //    .Add("Line A")
        //        .Add("Line A.A")
        //            .Add("Line A.A.B").Root
        //    .Add("Line B")
        //        .Add("Line B.B")
        //            .Add("Line B.A.A").Root
        //    .Add("Line D")
        //        .Add("Line D.A")
        //            .Add("Line D.A.A").Root;

        var comparision = ConfigurationSection.Compare(left.Children(), right.Children());
    }

    [Test]
    public void Contains01()
    {
        Assert.That(ConfigurationSection.Parse(TestData.ConfigurationFragment04).Contains("Line B"), Is.True);
    }

    [Test]
    public void Contains02()
    {
        Assert.That(ConfigurationSection.Parse(TestData.ConfigurationFragment04).Contains("Line B", "Line B.B", "Line B.A.A"), Is.True);
    }

    [Test]
    public void Depth01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.That(configuration.Depth, Is.EqualTo(0));
        Assert.That(configuration[0].Depth, Is.EqualTo(1));
        Assert.That(configuration[0][0].Depth, Is.EqualTo(2));
        Assert.That(configuration[0][0][0].Depth, Is.EqualTo(3));
    }

    [Test]
    public void Find01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var find = configuration.Find("item 5", "item 5.1", "item 5.5");

        Assert.That(find.Length, Is.EqualTo(1));
        Assert.That(find[0].Command, Is.EqualTo("item 5.5"));
    }

    [Test]
    public void Find02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var find = configuration.Find("item 5", "item 5.X", "item 5.5");

        Assert.That(find.Length, Is.EqualTo(0));
    }

    [Test]
    public void First01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        Assert.That(configuration["Line B"]["Line B.B"].Single().Command, Is.EqualTo("Line B.A.A"));
        Assert.That(configuration["Line D"].First().Command, Is.EqualTo("Line D.A"));
        Assert.That(configuration["Line D"]["Line D.A"].First().Command, Is.EqualTo("Line D.A.A"));
        Assert.Throws<InvalidOperationException>(() => _ = configuration["Line D"]["Line D.A"]["Line D.A.A"].First().Command);
    }

    [Test]
    public void Index01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.That(configuration[0].Command, Is.EqualTo("item 1"));
        Assert.That(configuration[1].Command, Is.EqualTo("banner mod ^"));
        Assert.That(configuration[2].Command, Is.EqualTo("item 2"));
        Assert.That(configuration[3].Command, Is.EqualTo("item 3"));
        Assert.That(configuration[4].Command, Is.EqualTo("item 4"));
    }

    [Test]
    public void Index02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.That(configuration["item 1"].Command, Is.EqualTo("item 1"));
        Assert.That(configuration["banner mod ^"].Command, Is.EqualTo("banner mod ^"));
        Assert.That(configuration["item 2"].Command, Is.EqualTo("item 2"));
        Assert.That(configuration["item 3"].Command, Is.EqualTo("item 3"));
        Assert.That(configuration["item 4"].Command, Is.EqualTo("item 4"));

        Assert.That(configuration["item 1"]["item 1.1"]["item 1.1.2"].Command, Is.EqualTo("item 1.1.2"));

        Assert.That(configuration["ITEM 1", StringComparison.CurrentCultureIgnoreCase].Command, Is.EqualTo("item 1"));

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var value = configuration["DOES NOT EXIST"];
        });
    }

    [Test]
    public void Indexer01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var command = configuration["item 1"]["item 1.3"].Command;

        Assert.That(command, Is.EqualTo("item 1.3"));
    }

    [Test]
    public void Indexer2()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        Assert.Throws<KeyNotFoundException>(() => { _ = configuration["Missing Key"]; });
    }

    [Test]
    public void Json01()
    {
        var configuration = ConfigurationSection.Parse("""
            No Line
            First Line
             Second Line
             Third Line
              Forth Line
            Fifth Line
            """);

        var jsonArray = configuration.AsJson();

        var expectedJson = """
            [
              "No Line",
              {
                "First Line": [
                  "Second Line",
                  {
                    "Third Line": [
                      "Forth Line"
                    ]
                  }
                ]
              },
              "Fifth Line"
            ]
            """;

        Assert.That(jsonArray.ToString(), Is.EqualTo(expectedJson));
    }

    [Test]
    public void Json02()
    {
        var configuration = ConfigurationSection.Parse("""
            Single Line
            """);

        var jsonArray = configuration.AsJson();

        var expectedJson = """
            [
              "Single Line"
            ]
            """;

        Assert.That(jsonArray.ToString(), Is.EqualTo(expectedJson));
    }

    [Test]
    public void Json03()
    {
        var configuration = ConfigurationSection.Parse(string.Empty);

        var jsonArray = configuration.AsJson();

        var expectedJson = """
            []
            """;

        Assert.That(jsonArray.ToString(), Is.EqualTo(expectedJson));
    }

    [Test]
    public void Json04()
    {
        var configuration = ConfigurationSection.Parse(TestData.RouterConfiguration01);

        var jsonArray = configuration.AsJson();

        var expectedJson = """
            [
              "!",
              "! Last configuration change at 01:15:54 UTC Thu Jun 13 2024 by admin",
              "!",
              "version 15.2",
              "service timestamps debug datetime msec",
              "service timestamps log datetime msec",
              "service password-encryption",
              "service compress-config",
              "!",
              "hostname switch02",
              "!",
              "boot-start-marker",
              "boot-end-marker",
              "!",
              "!",
              "enable password 7 105E080A16001D1908",
              "!",
              "username admin privilege 15 password 7 03145A1815182E5E4A",
              "aaa new-model",
              "!",
              "!",
              "aaa authentication login console local",
              "aaa authorization console",
              "aaa authorization exec default local",
              "aaa authorization exec console if-authenticated",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "aaa session-id common",
              "!",
              "!",
              "!",
              "dot1x system-auth-control",
              "!",
              {
                "class-map type control subscriber match-all DOT1X": [
                  "match method dot1x"
                ]
              },
              "!",
              {
                "class-map type control subscriber match-all DOT1X_FAILED": [
                  "match method dot1x",
                  "match result-type method dot1x authoritative"
                ]
              },
              "!",
              {
                "class-map type control subscriber match-all DOT1X_NO_RESP": [
                  "match method dot1x",
                  "match result-type method dot1x agent-not-found"
                ]
              },
              "!",
              {
                "class-map type control subscriber match-all MAB": [
                  "match method mab"
                ]
              },
              "!",
              {
                "class-map type control subscriber match-all MAB_FAILED": [
                  "match method mab",
                  "match result-type method mab authoritative"
                ]
              },
              "!",
              {
                "policy-map type control subscriber TEST4": [
                  {
                    "event session-started match-all": [
                      {
                        "10 class always do-until-failure": [
                          "10 authenticate using dot1x priority 10",
                          "20 authenticate using mab priority 20"
                        ]
                      }
                    ]
                  },
                  {
                    "event authentication-failure match-first": [
                      {
                        "10 class DOT1X_FAILED do-until-failure": [
                          "10 terminate dot1x"
                        ]
                      },
                      {
                        "20 class MAB_FAILED do-until-failure": [
                          "10 terminate mab",
                          "20 authenticate using dot1x priority 10"
                        ]
                      },
                      {
                        "30 class DOT1X_NO_RESP do-until-failure": [
                          "10 terminate dot1x",
                          "20 authentication-restart 60"
                        ]
                      },
                      {
                        "40 class always do-until-failure": [
                          "10 terminate mab",
                          "20 terminate dot1x",
                          "30 authentication-restart 60"
                        ]
                      }
                    ]
                  },
                  {
                    "event agent-found match-all": [
                      {
                        "10 class always do-until-failure": [
                          "10 terminate mab",
                          "20 authenticate using dot1x priority 10"
                        ]
                      }
                    ]
                  },
                  {
                    "event authentication-success match-all": [
                      {
                        "10 class always do-until-failure": [
                          "10 activate service-template DEFAULT_LINKSEC_POLICY_SHOULD_SECURE"
                        ]
                      }
                    ]
                  }
                ]
              },
              "!",
              "!",
              "!",
              "vtp domain test",
              "vtp mode off",
              "!",
              "!",
              "!",
              "ip cef",
              "no ipv6 cef",
              "!",
              "!",
              "!",
              "spanning-tree mode rapid-pvst",
              "spanning-tree extend system-id",
              "!",
              "!",
              {
                "vlan 20": [
                  "name multipoint"
                ]
              },
              "!",
              {
                "vlan 99": [
                  "name test"
                ]
              },
              "!",
              "vlan 123",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              {
                "interface Loopback100": [
                  "ip address 10.255.255.2 255.255.255.255",
                  "ip ospf 100 area 1"
                ]
              },
              "!",
              {
                "interface Port-channel1": [
                  "description test"
                ]
              },
              "!",
              {
                "interface GigabitEthernet0/0": [
                  "description switch02:Gi0/0",
                  "no switchport",
                  "ip address 10.255.0.1 255.255.255.254",
                  "ip address 10.255.99.2 255.255.255.254",
                  "ip address 10.255.99.3 255.255.255.254",
                  "ip address 10.255.99.4 255.255.255.254",
                  "ip ospf message-digest-key 1 md5 7 12090404011C03162E",
                  "ip ospf network point-to-point",
                  "ip ospf 100 area 1",
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet0/1": [
                  "description switch03:Gi0/1",
                  "no switchport",
                  "ip address 10.255.0.4 255.255.255.254",
                  "ip ospf message-digest-key 1 md5 7 105E080A16001D1908",
                  "ip ospf network point-to-point",
                  "ip ospf 100 area 1",
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet0/2": [
                  "description switch04:Gi0/1",
                  "no switchport",
                  "ip address 10.255.0.8 255.255.255.254",
                  "ip ospf message-digest-key 1 md5 7 051B071C325B411B1D",
                  "ip ospf network point-to-point",
                  "ip ospf 100 area 1",
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet0/3": [
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet1/0": [
                  "switchport access vlan 123",
                  "switchport mode access",
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet1/1": [
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet1/2": [
                  "switchport access vlan 20",
                  "negotiation auto"
                ]
              },
              "!",
              {
                "interface GigabitEthernet1/3": [
                  "description management",
                  "no switchport",
                  "ip address dhcp hostname switch01",
                  "negotiation auto",
                  "no cdp enable"
                ]
              },
              "!",
              {
                "interface Vlan20": [
                  "ip address 10.255.20.2 255.255.255.0",
                  "ip ospf message-digest-key 1 md5 7 12090404011C03162E",
                  "ip ospf 100 area 1"
                ]
              },
              "!",
              {
                "router ospf 100": [
                  "router-id 10.255.255.2",
                  "area 1 authentication message-digest",
                  "passive-interface default",
                  "no passive-interface GigabitEthernet0/0",
                  "no passive-interface GigabitEthernet0/1",
                  "no passive-interface GigabitEthernet0/2",
                  "no passive-interface GigabitEthernet0/3",
                  "no passive-interface Vlan20"
                ]
              },
              "!",
              "ip forward-protocol nd",
              "!",
              "ip http server",
              "ip http secure-server",
              "!",
              "ip ssh server algorithm encryption aes128-ctr aes192-ctr aes256-ctr",
              "ip ssh client algorithm encryption aes128-ctr aes192-ctr aes256-ctr",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "!",
              "control-plane",
              "!",
              "banner exec ^CC\r\n**************************************************************************\r\n*                          WELCOME TO Acme                               *\r\n*                                                                        *\r\n*       Authorized Access Only - All activities are monitored            *\r\n*                                                                        *\r\n*        Unauthorized access will be prosecuted to the fullest           *\r\n*                      extent of the law.                                *\r\n*                                                                        *\r\n*            For support, contact IT Support at:                         *\r\n*                Email: support@acme.com                                 *\r\n*                Phone: \u002B1 (800) 555-1234                                *\r\n*                                                                        *\r\n**************************************************************************\r\n^C",
              "banner incoming ^CC\r\n**************************************************************************\r\n*                          ACME CORPORATION                              *\r\n*                                                                        *\r\n*          You are accessing a restricted network device.                *\r\n*                                                                        *\r\n*            Unauthorized access is strictly prohibited.                 *\r\n*        All activities are logged and monitored for security.           *\r\n*                                                                        *\r\n*          For assistance, contact IT Support at:                        *\r\n*               Email: support@acme.com                                  *\r\n*               Phone: \u002B1 (800) 555-1234                                 *\r\n*                                                                        *\r\n**************************************************************************\r\n^C",
              "!",
              {
                "line con 0": [
                  "login authentication console"
                ]
              },
              "line aux 0",
              {
                "line vty 0 4": [
                  "exec-timeout 0 0",
                  "transport input ssh",
                  "transport output none"
                ]
              },
              "!",
              "!",
              "end"
            ]
            """;

        Assert.That(jsonArray.ToString(), Is.EqualTo(expectedJson));
    }

    [Test]
    public void Line01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment02);

        Assert.That(configuration[0].Line, Is.EqualTo("banner test1 ^\r\nline 1\r\nline 2\r\n line 3^"));
    }

    [Test]
    public void Match01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var matches = configuration.Match("^item .*", "", @"\.2$");

        Assert.That(matches[0].Section.Command, Is.EqualTo("item 1.1.2"));
        Assert.That(matches[0].Match.Value == ".2");

        Assert.That(matches[1].Section.Command, Is.EqualTo("item 1.3.2"));
        Assert.That(matches[1].Match.Value == ".2");

        Assert.That(matches[2].Section.Command, Is.EqualTo("item 5.2"));
        Assert.That(matches[2].Match.Value == ".2");
    }

    [Test]
    public void MatchTest02()
    {
        var section = ConfigurationSection.Parse(TestData.RouterConfiguration01);

        var sectionMatches = section.Match(@"^interface (?<interface>GigabitEthernet\d+/\d+)$", @"^ip address (?<address>\d+\.\d+\.\d+\.\d+) (?<mask>\d+\.\d+\.\d+\.\d+)$");

        Assert.That(sectionMatches.Length, Is.EqualTo(6));

        Assert.That(sectionMatches[0].Section!.Command, Is.EqualTo("ip address 10.255.0.1 255.255.255.254"));
        Assert.That(sectionMatches[0].Match!.Groups["address"].Value, Is.EqualTo("10.255.0.1"));

        Assert.That(sectionMatches[1].Section!.Command, Is.EqualTo("ip address 10.255.99.2 255.255.255.254"));
        Assert.That(sectionMatches[1].Match!.Groups["address"].Value, Is.EqualTo("10.255.99.2"));

        Assert.That(sectionMatches[2].Section!.Command, Is.EqualTo("ip address 10.255.99.3 255.255.255.254"));
        Assert.That(sectionMatches[2].Match!.Groups["address"].Value, Is.EqualTo("10.255.99.3"));

        Assert.That(sectionMatches[3].Section!.Command, Is.EqualTo("ip address 10.255.99.4 255.255.255.254"));
        Assert.That(sectionMatches[3].Match!.Groups["address"].Value, Is.EqualTo("10.255.99.4"));

        Assert.That(sectionMatches[4].Section!.Command, Is.EqualTo("ip address 10.255.0.4 255.255.255.254"));
        Assert.That(sectionMatches[4].Match!.Groups["address"].Value, Is.EqualTo("10.255.0.4"));

        Assert.That(sectionMatches[5].Section!.Command, Is.EqualTo("ip address 10.255.0.8 255.255.255.254"));
        Assert.That(sectionMatches[5].Match!.Groups["address"].Value, Is.EqualTo("10.255.0.8"));
    }

    [Test]
    public void Merge01()
    {
        var left = ConfigurationSection.Parse(TestData.ConfigurationFragment03);
        var right = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        var comparision = ConfigurationSection.Compare(left.Children(), right.Children());

        var zzz = comparision.Merge();
        var kkk = zzz.ToString();
    }

    [Test]
    public void Next01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment03);

        Assert.That(configuration["Line B"]["Line B.A"].Next().Command, Is.EqualTo("Line B.B"));
        Assert.That(configuration["Line C"].Next().Command, Is.EqualTo("Line E"));
        Assert.That(configuration["Line B"]["Line B.A"]["Line B.A.A"].Next().Command, Is.EqualTo("Line B.A.B"));
        Assert.Throws<IndexOutOfRangeException>(() => _ = configuration["Line C"]["Line C.A"].Next());
    }

    [Test]
    public void NextOrDefault01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment03);

        Assert.That(configuration["Line B"]["Line B.A"].NextOrDefault().Command, Is.EqualTo("Line B.B"));
        Assert.That(configuration["Line C"].NextOrDefault().Command, Is.EqualTo("Line E"));
        Assert.That(configuration["Line B"]["Line B.A"]["Line B.A.A"].NextOrDefault().Command, Is.EqualTo("Line B.A.B"));
        Assert.That(configuration["Line C"]["Line C.A"].NextOrDefault(), Is.Null);
    }

    [Test]
    public void ParseTest01()
    {
        var section = ConfigurationSection.Parse(TestData.RouterConfiguration01);

        Assert.That(section.Descendants().Count(), Is.EqualTo(222));

        Assert.That(section.ToString(), Is.EqualTo(TestData.RouterConfiguration01 + "\r\n"));
    }

    [Test]
    public void Path01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var path = configuration[0][0][2].Path;

        Assert.That(path, Is.EqualTo(@"\item 1\item 1.1\item 1.1.3"));
    }

    [Test]
    public void Previous01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment03);

        Assert.That(configuration["Line B"]["Line B.B"].Previous().Command, Is.EqualTo("Line B.A"));
        Assert.That(configuration["Line E"].Previous().Command, Is.EqualTo("Line C"));
        Assert.That(configuration["Line B"]["Line B.A"]["Line B.A.B"].Previous().Command, Is.EqualTo("Line B.A.A"));
        Assert.Throws<IndexOutOfRangeException>(() => _ = configuration["Line C"]["Line C.A"].Next());
    }

    [Test]
    public void PreviousOrDefault01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment03);

        Assert.That(configuration["Line B"]["Line B.B"].PreviousOrDefault().Command, Is.EqualTo("Line B.A"));
        Assert.That(configuration["Line E"].PreviousOrDefault().Command, Is.EqualTo("Line C"));
        Assert.That(configuration["Line B"]["Line B.A"]["Line B.A.B"].PreviousOrDefault().Command, Is.EqualTo("Line B.A.A"));
        Assert.That(configuration["Line C"]["Line C.A"].PreviousOrDefault(), Is.Null);
    }

    [Test]
    public void Remove01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line A"].Remove("Line A.A");
        configuration.Remove("Line B");
        configuration["Line D"]["Line D.A"].Remove("Line D.A.A");

        Assert.Throws<KeyNotFoundException>(() => configuration["Line D"]["Line D.A"].Remove("Line D.A.A"));

        var expected = """
            Line A
            Line D
             Line D.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Replace01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration[1][0].Replace("Test XXXX");

        Assert.That(configuration[1][0].Command, Is.EqualTo("Test XXXX"));
    }

    [Test]
    public void Replace02()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        configuration["Line B"]["Line B.B"].Replace("Line X.X", true);

        var expected = """
            Line A
             Line A.A
              Line A.A.B
            Line B
              Line X.X
               Line B.A.A
            Line D
             Line D.A
              Line D.A.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Search01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var search = configuration.Search("^item", "", @"\.2$");

        Assert.That(search[0].Command, Is.EqualTo("item 1.1.2"));
        Assert.That(search[1].Command, Is.EqualTo("item 1.3.2"));
        Assert.That(search[2].Command, Is.EqualTo("item 5.2"));
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ToStringEqualsOriginal()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment01);

        var stringValue = configuration.ToString();

        Assert.That(TestData.ConfigurationFragment01, Is.EqualTo(stringValue));
    }

    [Test]
    public void TryGet01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        ConfigurationSection? section;
        Assert.That(configuration["Line A"].TryGet("Line A.A", out section), Is.True);
        Assert.That(section.Command, Is.EqualTo("Line A.A"));

        Assert.That(configuration.TryGet("Line B", out section), Is.True);
        Assert.That(section.Command, Is.EqualTo("Line B"));

        Assert.That(configuration["Line D"]["Line D.A"].TryGet("Line D.A.A", out section), Is.True);
        Assert.That(section.Command, Is.EqualTo("Line D.A.A"));

        Assert.That(configuration["Line D"]["Line D.A"].TryGet("Line D.A.Z", out section), Is.False);
    }

    [Test]
    public void TryRemove01()
    {
        var configuration = ConfigurationSection.Parse(TestData.ConfigurationFragment04);

        Assert.That(configuration["Line A"].TryRemove("Line A.A"), Is.True);
        Assert.That(configuration.TryRemove("Line B"), Is.True);
        Assert.That(configuration["Line D"]["Line D.A"].TryRemove("Line D.A.A"), Is.True);

        Assert.That(configuration["Line D"]["Line D.A"].TryRemove("Line D.A.A"), Is.False);

        var expected = """
            Line A
            Line D
             Line D.A

            """;

        Assert.That(configuration.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Playground01()
    {
        var configuration = ConfigurationSection.Parse("""
            interface GigabitEthernet0/1
             ip address 192.168.1.1 255.255.255.0
             speed 1000
             duplex full
            !
            interface GigabitEthernet0/2
             ip address 192.168.1.2 255.255.255.0
             speed 1000
             duplex full
            !
            """);

        foreach (var configurationSection in configuration.Search("^interface GigabitEthernet")) 
            configurationSection.AddFirst("description Ethernet port");
    }
}