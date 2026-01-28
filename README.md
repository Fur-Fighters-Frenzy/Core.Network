# Core.Network

Small, transport- and engine-agnostic networking core for Unity:
- Primitive IDs (`PlayerId`)
- Unified channel abstraction (`ChannelKind`)
- Minimal transport interfaces (`INetClient`, `INetServer`)
- Optional adapters for [**FishNet**](https://github.com/Fur-Fighters-Frenzy/Core.Network.FishNet) and [**LiteNetLib**](https://github.com/Fur-Fighters-Frenzy/Core.Network.LiteNetLib)
- Tiny binary handshake DTO (`HandshakeDto`)

> **Status:** WIP

---

## Why this exists

Unity networking stacks love to dictate your architecture. This package does the opposite: it provides a small set of primitives and contracts so your higher-level code (replication, envelopes, state sync) doesn’t depend on a specific transport.

---

## What’s inside

### Transport abstraction
- `INetClient`: `Send(...)`, `OnServerMessage`, `OnConnectedAsClient`
- `INetServer`: `Send(...)`, `Broadcast(...)`, `BroadcastExcept(...)`, `Poll()`, connect/disconnect events

### Channels
`ChannelKind` is a unified channel model across transports:
- `ReliableOrdered`
- `Unreliable`
- `Sequenced` (relevant for LiteNetLib; on FishNet it’s mapped to best-effort behavior)

### Adapters
- [**LiteNetLib**](https://github.com/Fur-Fighters-Frenzy/Core.Network.LiteNetLib)
    - `LiteClientAdapter`, `LiteServerAdapter`
    - `LiteChannelMap` for `(DeliveryMethod, channelNumber) <-> ChannelKind` mapping
- [**FishNet**](https://github.com/Fur-Fighters-Frenzy/Core.Network.FishNet)
    - `FishBridge` (partial): client/server bridge via RPC
    - `FishChannelMap` (`Reliable/Unreliable` <-> `ChannelKind`)

### Handshake
`HandshakeDto`: fixed-size binary format (27 bytes)  
`[u64 matchId][u16 seq][u8 playerId][guid token(16)]` (little-endian)

---

## Scripting define symbols

Adapters are only compiled when the corresponding define symbol is present:
- `LITENETLIB_ENABLED`
- `FISHNET_ENABLED`

---

## Example usage (high-level)

```csharp
INetServer server = /* LiteServerAdapter or FishBridge (server side) */;
server.OnClientConnected += pid => { /* ... */ };
server.OnClientMessage += (pid, data, ch) => { /* decode envelope/DTO */ };

// In your tick/update loop:
server.Poll(); // LiteNetLib needs Poll; FishNet is a no-op here
```

---

# Part of the Core Project

This package is part of the **Core** project, which consists of multiple Unity packages.
See the full project here: [Core](https://github.com/Fur-Fighters-Frenzy/Core)

---