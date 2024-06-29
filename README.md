# Bitvantage.Cisco.ConfigurationSection
Facilitate parsing configuration files from Cisco routers and switches into a document object model (DOM) enabling network administrators and developers to easily read, query, and modify the structured data from Cisco device configurations.
## Installing via NuGet Package Manager
```
PM> NuGet\Install-Package Bitvantage.Cisco.ConfigurationSection
```

## Quick Start
```csharp
 var configuration => """
        !
        ! Last configuration change at 01:15:54 UTC Thu Jun 13 2024 by admin
        !
        version 15.2
        service timestamps debug datetime msec
        service timestamps log datetime msec
        service password-encryption
        service compress-config
        """;

var configurationSection = ConfigurationSection.Parse(configuration);

configurationSection
    .Add("class-map type control subscriber match-all DOT1X")
    .Add("match method dot1x");

configurationSection.Remove("service compress-config");

var updatedConfiguration = configurationSection.ToString();
```

## Working with Nodes
Each configuration command is represented by a node in a tree with sub-commands represented as child nodes. For example:
```
configuration
├── interface GigabitEthernet0/1
│   ├── description Uplink to Core Switch
│   ├── ip address 192.168.1.1 255.255.255.0
│   ├── speed 1000
│   └── duplex full
├── interface GigabitEthernet0/2
│   ├── description Connection to Server
│   ├── ip address 192.168.1.2 255.255.255.0
│   ├── speed 1000
│   └── duplex full
├── vlan10
│   └── name Sales
├── vlan20
│   └── name Engineering
└── ip route 100.0.0.0 255.255.255.0 gigabitEthernet 0/1
```

### Programicly Building a Configuration
A ConfigurationSection section, more or less, acts as a list that contains children, when you add a child node to a node, the added node is returned.
```csharp
var configuration = new ConfigurationSection()
    .Add("interface GigabitEthernet0/1")
    .Add("ip address 192.168.1.1 255.255.255.0").Parent
    .Add("speed 1000").Parent
    .Add("duplex full").Root
    .Add("interface GigabitEthernet0/2")
    .Add("ip address 192.168.1.2 255.255.255.0").Parent
    .Add("speed 1000").Parent
    .Add("duplex full")  
```

### Updating a Node
Nodes are immutable; however an existing node can be replaced by another node
```csharp
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

configuration["interface GigabitEthernet0/2"]["ip address 192.168.1.2 255.255.255.0"].Replace("ip address 192.168.100.2 255.255.255.0");
```

## Adding a Node
```csharp
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
```

### Getting a Child Node By Exact Name
```csharp
// gets the first matching child node; throws an exception if the node does not exist
var childNode1 = configurationSection["spanning-tree mode rapid-pvst"]

// tries to get the first matching child node
var success = configurationSection.TryGet("spanning-tree mode rapid-pvst", out var childNode2);
```

### Finding Child Nodes by Exact Name
To find nodes by exact match use the Find() method. Multiple names can be specified, which correspond to the node path.

```csharp
var matchingNodes = configurationSection.Find("interface GigabitEtherne1/0","ip address 10.255.0.4 255.255.255.254");
```

### Searching Child Nodes by Regular Expression
To search for nodes by a regular expression use the Search() method. Multiple regular expressions can be specified, which correspond to the path of the node.

For example to find GigabitEthernet interfaces that have an IPv4 address set:
```csharp
var matchingNodes = configurationSection.Search(@"^interface GigabitEthernet\d+/\d+","^ip address \d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3} \d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
```

### Matching Child Nodes by Regular Expression
To find nodes by regular expression and retain the matched groups for future use, use the Match() method. Multiple regular expressions can be specified, which correspond to the path of the node.
```csharp
var sectionMatches = configurationSection.Match(@"^interface (?<interface>GigabitEthernet\d+/\d+)$", @"^ip address (?<network>(?<address>\d+\.\d+\.\d+\.\d+) (?<mask>\d+\.\d+\.\d+\.\d+))$");

var interfaces = sectionMatches
    .Select(item => new
    {
        Interface = IosInterface.Parse(item.Path[0].Match.Groups["interface"].Value),
        Address = IPAddress.Parse(item.Match.Groups["address"].Value),
        Network = Network.Parse(item.Match.Groups["network"].Value)
    }).ToList();
```