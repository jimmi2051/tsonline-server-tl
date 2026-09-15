# tsonline-server-tl | Source Server TSOnline

A private **TS Online** MMORPG game server written in C# on .NET Framework 4.8. It implements the game protocol as a raw TCP socket server with packet handlers, backed by a MySQL database.

**Author:** Lý Thành — thanhnl0697@gmail.com

## Support / Donation

If this project helps you, consider buying me a coffee:

👉 **[Donate via PayPal](https://www.paypal.com/ncp/payment/AAUBZ6U4MR9GN)**

Thank you for the support! ❤️

## Features

- Raw TCP socket server (listens on port **6414**)
- Packet-based protocol: reader, processor, creator, and per-feature handlers
- MySQL persistence for accounts, characters, items, and world state
- Game systems: authentication, character creation, movement, chat, parties/groups, battle (PvP, NPC), pets, skills/stats, inventory & storage, warps, and NPC/EVE scripting
- Static game data loaded from local data files (items, NPCs, warps, skills, scenes, talk/EVE data)

## Tech Stack

- **Language / Runtime:** C# — .NET Framework 4.8
- **Database:** MySQL (`MySql.Data`)
- **Libraries:** Newtonsoft.Json, EntityFramework, log4net, Munq IoC Container
- **IDE:** Visual Studio (solution `TS_Server.sln`)

## Project Structure

```
TS_Server/
├── TSServer.cs            Entry point (Main), boots world + socket handler
├── ServerHandler.cs       TCP socket accept/receive loop
├── PacketReader/Processor/Creator.cs   Protocol serialization
├── PacketHandlers/        Per-feature handlers (Auth, Battle, Chat, Item, Move, Party, Pet, ...)
├── Server/                World, maps, battle logic (PvP / NPC)
├── Client/                Player-side models (Character, Item, Pet, Party, ...)
├── DataTools/             Static game-data loaders (Item, Npc, Warp, Skill, Scene, ...)
├── TSMysqlConnection.cs   MySQL connection + query helpers
└── TSConstants.cs         Constants
SQL/                       Database schema (TS_Online.sql) + Wipe.sql
ThirdParty/                Bundled DLLs (EntityFramework, log4net, Munq, MySql)
```

## Getting Started

### Prerequisites

- Windows + Visual Studio (2012 or later) with .NET Framework 4.8
- MySQL server

### Setup

1. **Create the database** — import the schema:

   ```sql
   SOURCE SQL/TS_Online.sql;
   ```

2. **Configure the DB connection** — create `dbconfig.txt` next to the server executable. Fields are tab-separated (`key<TAB>value`); the first line is a header:

   ```
   config	value
   server	127.0.0.1
   database	ts_online
   uid	root
   password	yourpassword
   port	3306
   ```

3. **Provide the game data files** in the server working directory (e.g. `warps.txt`, item/NPC/skill/EVE data). These are read on startup.

4. **Build & run** — open `TS_Server.sln`, build, and run. The server loads game data then listens on port **6414**.

To reset the world state, run `SQL/Wipe.sql`.

## Disclaimer

Educational / preservation project for the TS Online game. Not affiliated with the original publisher. Use at your own risk.
