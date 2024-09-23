# AAC Light-Heavyweight M4 (Savage) - "Wicked Thunder"

Contributions by `Akechi` & `Veyn`.

## Main-Tank (T1/MT)

### GNB

```
{
  "Name": "MT",
  "Encounter": "BossMod.Dawntrail.Savage.M04SWickedThunder.M04SWickedThunder",
  "Class": "GNB",
  "Level": 100,
  "PhaseDurations": [
    812.2
  ],
  "Modules": {
    "BossMod.Autorotation.ClassGNBUtility": {
      "LB": [
        {
          "Option": "LB3",
          "StateID": "0x000C0031",
          "TimeSinceActivation": 4.5,
          "WindowLength": 3.9
        }
      ],
      "Rampart": [
        {
          "Option": "Use",
          "StateID": "0x00000001",
          "TimeSinceActivation": 1.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00050001",
          "TimeSinceActivation": 1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090100",
          "TimeSinceActivation": 4.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000B0200",
          "TimeSinceActivation": 3.4,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180040",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00130013",
          "TimeSinceActivation": 3.3,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001B0000",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        }
      ],
      "Provoke": [
        {
          "Option": "Use",
          "StateID": "0x00090302",
          "TimeSinceActivation": 3.5,
          "WindowLength": 3,
          "Target": "EnemyByOID",
          "TargetParam": 17322
        }
      ],
      "Reprisal": [
        {
          "Option": "UseEx",
          "StateID": "0x00000000",
          "TimeSinceActivation": 1,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00030001",
          "TimeSinceActivation": 11,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00050031",
          "TimeSinceActivation": 4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00070051",
          "TimeSinceActivation": 3.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00090201",
          "TimeSinceActivation": 4.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x000B0103",
          "TimeSinceActivation": 0.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x000C0031",
          "TimeSinceActivation": 1.5,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00160002",
          "TimeSinceActivation": 1,
          "WindowLength": 3.1
        },
        {
          "Option": "UseEx",
          "StateID": "0x00180063",
          "TimeSinceActivation": 3.2,
          "WindowLength": 3.1
        },
        {
          "Option": "UseEx",
          "StateID": "0x001A0061",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00130001",
          "TimeSinceActivation": 5.9,
          "WindowLength": 3.1
        }
      ],
      "Shirk": [
        {
          "Option": "Use",
          "StateID": "0x000B0000",
          "TimeSinceActivation": 3.3,
          "WindowLength": 4
        }
      ],
      "Stance": [
        {
          "Option": "Apply",
          "StateID": "0x00090301",
          "TimeSinceActivation": 5.6,
          "WindowLength": 4
        },
        {
          "Option": "Remove",
          "StateID": "0x000A0003",
          "TimeSinceActivation": 2.7,
          "WindowLength": 4
        },
        {
          "Option": "Apply",
          "StateID": "0x00000000",
          "TimeSinceActivation": -8,
          "WindowLength": 8
        }
      ],
      "Camouflage": [
        {
          "Option": "Use",
          "StateID": "0x00010002",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090010",
          "TimeSinceActivation": 1.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000B0100",
          "TimeSinceActivation": 2.8,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0050",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00150011",
          "TimeSinceActivation": 2.6,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180047",
          "TimeSinceActivation": 0,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001B0020",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3.1
        }
      ],
      "Nebula": [
        {
          "Option": "Use",
          "StateID": "0x00090202",
          "TimeSinceActivation": 4.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00190001",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00120001",
          "TimeSinceActivation": 2.3,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001D0000",
          "TimeSinceActivation": 0.3,
          "WindowLength": 3.1
        }
      ],
      "Aurora": [
        {
          "Option": "Use",
          "StateID": "0x00060000",
          "TimeSinceActivation": 1.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090302",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3.7
        },
        {
          "Option": "Use",
          "StateID": "0x00090102",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0030",
          "TimeSinceActivation": 5.6,
          "WindowLength": 3.1,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "Use",
          "StateID": "0x00190000",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3.1
        }
      ],
      "Superbolide": [
        {
          "Option": "Use",
          "StateID": "0x00060001",
          "TimeSinceActivation": 3,
          "WindowLength": 2
        },
        {
          "Option": "Use",
          "StateID": "0x000A0001",
          "TimeSinceActivation": 3.6,
          "WindowLength": 2.1
        }
      ],
      "HeartOfLight": [
        {
          "Option": "Use",
          "StateID": "0x00020010",
          "TimeSinceActivation": 0,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00070031",
          "TimeSinceActivation": 1.5,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090300",
          "TimeSinceActivation": 1.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0000",
          "TimeSinceActivation": 4.4,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00140000",
          "TimeSinceActivation": 2.8,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180011",
          "TimeSinceActivation": 1.2,
          "WindowLength": 3.1
        },
        {
          "Option": "Use",
          "StateID": "0x001A0011",
          "TimeSinceActivation": 2,
          "WindowLength": 3
        }
      ],
      "HeartOfCorundum": [
        {
          "Option": "HoC",
          "StateID": "0x00070000",
          "TimeSinceActivation": 3.5,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00090012",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00090301",
          "TimeSinceActivation": 2.7,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0106",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0206",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x000C0050",
          "TimeSinceActivation": 5.2,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00120001",
          "TimeSinceActivation": 5.3,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00130013",
          "TimeSinceActivation": 0.5,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00190001",
          "TimeSinceActivation": 5.2,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00160011",
          "TimeSinceActivation": 4,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00180020",
          "TimeSinceActivation": 1.6,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x001A0020",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x001A0080",
          "TimeSinceActivation": 4.2,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x001C0010",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00180042",
          "TimeSinceActivation": 2.5,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0000",
          "TimeSinceActivation": 8.9,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00010000",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00020011",
          "TimeSinceActivation": 2,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00030020",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00050000",
          "TimeSinceActivation": 5.4,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00070041",
          "TimeSinceActivation": 4,
          "WindowLength": 2.9,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00150002",
          "TimeSinceActivation": 1.8,
          "WindowLength": 3,
          "Target": "Self"
        }
      ]
    }
  },
  "Targeting": []
}
```


## Off-Tank (T2/OT)

### GNB

```
{
  "Name": "OT",
  "Encounter": "BossMod.Dawntrail.Savage.M04SWickedThunder.M04SWickedThunder",
  "Class": "GNB",
  "Level": 100,
  "PhaseDurations": [
    812.2
  ],
  "Modules": {
    "BossMod.Autorotation.ClassGNBUtility": {
      "LB": [
        {
          "Option": "LB3",
          "StateID": "0x000C0031",
          "TimeSinceActivation": 4.5,
          "WindowLength": 3.9
        }
      ],
      "Rampart": [
        {
          "Option": "Use",
          "StateID": "0x00000001",
          "TimeSinceActivation": 1.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00050001",
          "TimeSinceActivation": 1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090100",
          "TimeSinceActivation": 4.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000B0200",
          "TimeSinceActivation": 3.4,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180040",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00130013",
          "TimeSinceActivation": 3.3,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001B0000",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        }
      ],
      "Provoke": [
        {
          "Option": "Use",
          "StateID": "0x00090302",
          "TimeSinceActivation": 3.5,
          "WindowLength": 3,
          "Target": "EnemyByOID",
          "TargetParam": 17322
        }
      ],
      "Reprisal": [
        {
          "Option": "UseEx",
          "StateID": "0x00000000",
          "TimeSinceActivation": 1,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00030001",
          "TimeSinceActivation": 11,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00050031",
          "TimeSinceActivation": 4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00070051",
          "TimeSinceActivation": 3.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00090201",
          "TimeSinceActivation": 4.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x000B0103",
          "TimeSinceActivation": 0.4,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x000C0031",
          "TimeSinceActivation": 1.5,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00160002",
          "TimeSinceActivation": 1,
          "WindowLength": 3.1
        },
        {
          "Option": "UseEx",
          "StateID": "0x00180063",
          "TimeSinceActivation": 3.2,
          "WindowLength": 3.1
        },
        {
          "Option": "UseEx",
          "StateID": "0x001A0061",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "UseEx",
          "StateID": "0x00130001",
          "TimeSinceActivation": 5.9,
          "WindowLength": 3.1
        }
      ],
      "Shirk": [
        {
          "Option": "Use",
          "StateID": "0x000B0000",
          "TimeSinceActivation": 3.3,
          "WindowLength": 4
        }
      ],
      "Stance": [
        {
          "Option": "Apply",
          "StateID": "0x00090301",
          "TimeSinceActivation": 5.6,
          "WindowLength": 4
        },
        {
          "Option": "Remove",
          "StateID": "0x000A0003",
          "TimeSinceActivation": 2.7,
          "WindowLength": 4
        },
        {
          "Option": "Remove",
          "StateID": "0x00000000",
          "TimeSinceActivation": -8,
          "WindowLength": 8
        }
      ],
      "Camouflage": [
        {
          "Option": "Use",
          "StateID": "0x00010002",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090010",
          "TimeSinceActivation": 1.1,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000B0100",
          "TimeSinceActivation": 2.8,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0050",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00150011",
          "TimeSinceActivation": 2.6,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180047",
          "TimeSinceActivation": 0,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001B0020",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3.1
        }
      ],
      "Nebula": [
        {
          "Option": "Use",
          "StateID": "0x00090202",
          "TimeSinceActivation": 4.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00190001",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00120001",
          "TimeSinceActivation": 2.3,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x001D0000",
          "TimeSinceActivation": 0.3,
          "WindowLength": 3.1
        }
      ],
      "Aurora": [
        {
          "Option": "Use",
          "StateID": "0x00060000",
          "TimeSinceActivation": 1.2,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090302",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3.7
        },
        {
          "Option": "Use",
          "StateID": "0x00090102",
          "TimeSinceActivation": 0.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0030",
          "TimeSinceActivation": 5.6,
          "WindowLength": 3.1,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "Use",
          "StateID": "0x00190000",
          "TimeSinceActivation": 2.2,
          "WindowLength": 3.1
        }
      ],
      "Superbolide": [
        {
          "Option": "Use",
          "StateID": "0x00060001",
          "TimeSinceActivation": 1,
          "WindowLength": 3,
          "Disabled": true
        },
        {
          "Option": "Use",
          "StateID": "0x000A0001",
          "TimeSinceActivation": 3.6,
          "WindowLength": 2.1
        }
      ],
      "HeartOfLight": [
        {
          "Option": "Use",
          "StateID": "0x00020010",
          "TimeSinceActivation": 0,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00070031",
          "TimeSinceActivation": 1.5,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00090300",
          "TimeSinceActivation": 1.7,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x000C0000",
          "TimeSinceActivation": 4.4,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00140000",
          "TimeSinceActivation": 2.8,
          "WindowLength": 3
        },
        {
          "Option": "Use",
          "StateID": "0x00180011",
          "TimeSinceActivation": 1.2,
          "WindowLength": 3.1
        },
        {
          "Option": "Use",
          "StateID": "0x001A0011",
          "TimeSinceActivation": 2,
          "WindowLength": 3
        }
      ],
      "HeartOfCorundum": [
        {
          "Option": "HoC",
          "StateID": "0x00070000",
          "TimeSinceActivation": 1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00090012",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00090301",
          "TimeSinceActivation": 2.7,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0106",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0206",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x000C0050",
          "TimeSinceActivation": 5.2,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00120001",
          "TimeSinceActivation": 5.3,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00130013",
          "TimeSinceActivation": 0.5,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00190001",
          "TimeSinceActivation": 5.2,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x00160011",
          "TimeSinceActivation": 4,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00180020",
          "TimeSinceActivation": 1.6,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x001A0020",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x001A0080",
          "TimeSinceActivation": 4.2,
          "WindowLength": 3,
          "Target": "Self"
        },
        {
          "Option": "HoC",
          "StateID": "0x001C0010",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00180042",
          "TimeSinceActivation": 2.5,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x000B0000",
          "TimeSinceActivation": 8.9,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00010000",
          "TimeSinceActivation": 0.1,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00020011",
          "TimeSinceActivation": 2,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00030020",
          "TimeSinceActivation": 0.6,
          "WindowLength": 3,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00050000",
          "TimeSinceActivation": 5.4,
          "WindowLength": 3
        },
        {
          "Option": "HoC",
          "StateID": "0x00070041",
          "TimeSinceActivation": 4,
          "WindowLength": 2.9,
          "Target": "PartyWithLowestHP"
        },
        {
          "Option": "HoC",
          "StateID": "0x00150002",
          "TimeSinceActivation": 1.8,
          "WindowLength": 3
        }
      ]
    }
  },
  "Targeting": []
}