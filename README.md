# SMOO

A custom UDP server for Super Mario Odyssey online. It manages rooms of players and relays game state between them.

Focuses on high performance for a smooth experience. 

Written in **C#** with **.NET 10**.

## Build & Run

```sh
dotnet build
dotnet run --project SMOO
```

The server listens on **UDP** port **5001**. Configuration is currently hardcoded.

## Client

The game client is a **C++** mod built on top of [Exlaunch](https://github.com/lynxdev2/smo-exlaunch-base-clang) that connects to this server. It currently isn't publicly available.

## Architecture

TBD
