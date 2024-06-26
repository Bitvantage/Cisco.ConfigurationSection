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

namespace Test;

internal static class TestData
{
    public static string ConfigurationFragment01 => """
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
        end

        """;

    public static string ConfigurationFragment02 => """
        banner test1 ^
        line 1
        line 2
         line 3^
        banner test2 X
        line 1
        line 2X
        banner test3 X
        line 1
        line 2
        X
        banner test4 ^Cfasfasfasdf
        line 1
        ^C

        """;

    public static string ConfigurationFragment03 => """
        Line A
         Line A.A
          Line A.A.A
        Line B
         Line B.A
          Line B.A.A
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

    public static string ConfigurationFragment04 => """
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

    public static string ConfigurationFragment05 => """
        Line E
         Line E.A
          Line E.A.B
        Line F
         Line F.B
          Line F.A.A
        Line G
         Line G.A
          Line G.A.A

        """;

    public static string RouterConfiguration01 => """
        !
        ! Last configuration change at 01:15:54 UTC Thu Jun 13 2024 by admin
        !
        version 15.2
        service timestamps debug datetime msec
        service timestamps log datetime msec
        service password-encryption
        service compress-config
        !
        hostname switch02
        !
        boot-start-marker
        boot-end-marker
        !
        !
        enable password 7 105E080A16001D1908
        !
        username admin privilege 15 password 7 03145A1815182E5E4A
        aaa new-model
        !
        !
        aaa authentication login console local
        aaa authorization console
        aaa authorization exec default local
        aaa authorization exec console if-authenticated
        !
        !
        !
        !
        !
        !
        aaa session-id common
        !
        !
        !
        dot1x system-auth-control
        !
        class-map type control subscriber match-all DOT1X
         match method dot1x
        !
        class-map type control subscriber match-all DOT1X_FAILED
         match method dot1x
         match result-type method dot1x authoritative
        !
        class-map type control subscriber match-all DOT1X_NO_RESP
         match method dot1x
         match result-type method dot1x agent-not-found
        !
        class-map type control subscriber match-all MAB
         match method mab
        !
        class-map type control subscriber match-all MAB_FAILED
         match method mab
         match result-type method mab authoritative
        !
        policy-map type control subscriber TEST4
         event session-started match-all
          10 class always do-until-failure
           10 authenticate using dot1x priority 10
           20 authenticate using mab priority 20
         event authentication-failure match-first
          10 class DOT1X_FAILED do-until-failure
           10 terminate dot1x
          20 class MAB_FAILED do-until-failure
           10 terminate mab
           20 authenticate using dot1x priority 10
          30 class DOT1X_NO_RESP do-until-failure
           10 terminate dot1x
           20 authentication-restart 60
          40 class always do-until-failure
           10 terminate mab
           20 terminate dot1x
           30 authentication-restart 60
         event agent-found match-all
          10 class always do-until-failure
           10 terminate mab
           20 authenticate using dot1x priority 10
         event authentication-success match-all
          10 class always do-until-failure
           10 activate service-template DEFAULT_LINKSEC_POLICY_SHOULD_SECURE
        !
        !
        !
        vtp domain test
        vtp mode off
        !
        !
        !
        ip cef
        no ipv6 cef
        !
        !
        !
        spanning-tree mode rapid-pvst
        spanning-tree extend system-id
        !
        !
        vlan 20
         name multipoint
        !
        vlan 99
         name test
        !
        vlan 123
        !
        !
        !
        !
        !
        !
        !
        !
        !
        !
        !
        !
        !
        !
        interface Loopback100
         ip address 10.255.255.2 255.255.255.255
         ip ospf 100 area 1
        !
        interface Port-channel1
         description test
        !
        interface GigabitEthernet0/0
         description switch02:Gi0/0
         no switchport
         ip address 10.255.0.1 255.255.255.254
         ip address 10.255.99.2 255.255.255.254
         ip address 10.255.99.3 255.255.255.254
         ip address 10.255.99.4 255.255.255.254
         ip ospf message-digest-key 1 md5 7 12090404011C03162E
         ip ospf network point-to-point
         ip ospf 100 area 1
         negotiation auto
        !
        interface GigabitEthernet0/1
         description switch03:Gi0/1
         no switchport
         ip address 10.255.0.4 255.255.255.254
         ip ospf message-digest-key 1 md5 7 105E080A16001D1908
         ip ospf network point-to-point
         ip ospf 100 area 1
         negotiation auto
        !
        interface GigabitEthernet0/2
         description switch04:Gi0/1
         no switchport
         ip address 10.255.0.8 255.255.255.254
         ip ospf message-digest-key 1 md5 7 051B071C325B411B1D
         ip ospf network point-to-point
         ip ospf 100 area 1
         negotiation auto
        !
        interface GigabitEthernet0/3
         negotiation auto
        !
        interface GigabitEthernet1/0
         switchport access vlan 123
         switchport mode access
         negotiation auto
        !
        interface GigabitEthernet1/1
         negotiation auto
        !
        interface GigabitEthernet1/2
         switchport access vlan 20
         negotiation auto
        !
        interface GigabitEthernet1/3
         description management
         no switchport
         ip address dhcp hostname switch01
         negotiation auto
         no cdp enable
        !
        interface Vlan20
         ip address 10.255.20.2 255.255.255.0
         ip ospf message-digest-key 1 md5 7 12090404011C03162E
         ip ospf 100 area 1
        !
        router ospf 100
         router-id 10.255.255.2
         area 1 authentication message-digest
         passive-interface default
         no passive-interface GigabitEthernet0/0
         no passive-interface GigabitEthernet0/1
         no passive-interface GigabitEthernet0/2
         no passive-interface GigabitEthernet0/3
         no passive-interface Vlan20
        !
        ip forward-protocol nd
        !
        ip http server
        ip http secure-server
        !
        ip ssh server algorithm encryption aes128-ctr aes192-ctr aes256-ctr
        ip ssh client algorithm encryption aes128-ctr aes192-ctr aes256-ctr
        !
        !
        !
        !
        !
        !
        !
        !
        control-plane
        !
        banner exec ^CC
        **************************************************************************
        *                          WELCOME TO Acme                               *
        *                                                                        *
        *       Authorized Access Only - All activities are monitored            *
        *                                                                        *
        *        Unauthorized access will be prosecuted to the fullest           *
        *                      extent of the law.                                *
        *                                                                        *
        *            For support, contact IT Support at:                         *
        *                Email: support@acme.com                                 *
        *                Phone: +1 (800) 555-1234                                *
        *                                                                        *
        **************************************************************************
        ^C
        banner incoming ^CC
        **************************************************************************
        *                          ACME CORPORATION                              *
        *                                                                        *
        *          You are accessing a restricted network device.                *
        *                                                                        *
        *            Unauthorized access is strictly prohibited.                 *
        *        All activities are logged and monitored for security.           *
        *                                                                        *
        *          For assistance, contact IT Support at:                        *
        *               Email: support@acme.com                                  *
        *               Phone: +1 (800) 555-1234                                 *
        *                                                                        *
        **************************************************************************
        ^C
        !
        line con 0
         login authentication console
        line aux 0
        line vty 0 4
         exec-timeout 0 0
         transport input ssh
         transport output none
        !
        !
        end
        """;
}