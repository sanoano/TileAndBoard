# Cardestia Function Documentation

Welcome to hell. Every major function found in the game is listed here with its signature and a description. Still a WIP.

## AudioManager

Source: `Assets/Scripts/Audio.cs`

### `PlaySound(string name)`

**Access:** Public

```csharp
public void PlaySound(string name)
```

**Description / comments:**

<!-- Add description/comments here. -->

Just pass in the name of the sound as a string. No need to pass in the path.

### `PlaySound(string name, bool randomisePitch)`

**Access:** Public

```csharp
public void PlaySound(string name, bool randomisePitch)
```

**Description / comments:**

<!-- Add description/comments here. -->

This is the same as above but call this one if you want to randomise the pitch of the sound every time it is played. Often for sounds that are repeated often.

### `PlaySound(string name, bool randomisePitch, float volume)`

**Access:** Public

```csharp
public void PlaySound(string name, bool randomisePitch, float volume)
```

**Description / comments:**

<!-- Add description/comments here. -->

The same as above but you can specify a volume for the sound to be played at from 0.0 to 1.0 (i think)

### `DestroyInstance(GameObject go, float time)`

**Access:** Private

```csharp
private static IEnumerator DestroyInstance(GameObject go, float time)
```

**Description / comments:**

<!-- Add description/comments here. -->

This is called automatically to clean stuff up, don't worry about it.


## BoardManager

Sources: `Assets/Scripts/BoardManager.cs`, `Assets/Scripts/BoardManager.Collections.cs`, and `Assets/Scripts/BoardManager.TileHelpers.cs`

### `NullSelection()`

**Access:** Public

```csharp
public void NullSelection()
```

**Description / comments:**

<!-- Add description/comments here. -->

Calling this clears the currently selected tile i.e. sets currentlyselectedtile to Null. Called when a tile is deseleted or an action is completed.

### `Defending()`

**Access:** Public

```csharp
public void Defending()
```

**Description / comments:**

<!-- Add description/comments here. -->

This is run every frame while the player is in the process of placing a defence instance.

### `MoveCard(int direction)`

**Access:** Public

```csharp
public void MoveCard(int direction)
```

**Description / comments:**

<!-- Add description/comments here. -->

This is run when a card is moved during a move action.

### `MoveCardRpc(int index, Vector2Int newPos, RpcParams rpcParams = default)`

**Access:** Public

```csharp
public void MoveCardRpc(int index, Vector2Int newPos, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

Called automatically if network is present upon moving

### `AddDefenseInstanceRpc(DefenseInstance instance, RpcParams rpcParams = default)`

**Access:** Public

```csharp
public void AddDefenseInstanceRpc(DefenseInstance instance, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

Called automatically if network is present upon placing Defense Instance.

### `AddDefenseInstanceLocal(DefenseInstance instance)`

**Access:** Public

```csharp
public void AddDefenseInstanceLocal(DefenseInstance instance)
```

**Description / comments:**

<!-- Add description/comments here. -->

Non-network version of AddDefenseInstance

### `AddDamageInstanceRpc(DamageInstance instance, RpcParams rpcParams = default)`

**Access:** Public

```csharp
public void AddDamageInstanceRpc(DamageInstance instance, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

Called automatically if network is present upon placing Damage Instance.


### `AddDamageInstanceLocal(DamageInstance instance)`

**Access:** Public

```csharp
public void AddDamageInstanceLocal(DamageInstance instance)
```

**Description / comments:**

<!-- Add description/comments here. -->

Non-network version of AddDamageInstance

### `UpdateTileVisuals()`

**Access:** Public

```csharp
public void UpdateTileVisuals()
```

**Description / comments:**

<!-- Add description/comments here. -->

Updates the colour of tiles and floating numbers to latest values. Needs to be called whenever you changing any board data like attack or defense instances or when previewing an attack,

### `PlaceCard(GameObject cardVisual, CardDeck.CardData cardData, GameObject tile)`

**Access:** Public

```csharp
public void PlaceCard(GameObject cardVisual, CardDeck.CardData cardData, GameObject tile)
```

**Description / comments:**

<!-- Add description/comments here. -->

Places a card. Incredible. Have to pass in the card visual and the data. Can be found by indexing into the arrays in card manager.

### `PlaceCardRpc(Unit unit, RpcParams rpcParams = default)`

**Access:** Public

```csharp
public void PlaceCardRpc(Unit unit, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

Called automatically if network is present.

### `PrepareAttack()`

**Access:** Public

```csharp
public void PrepareAttack()
```

**Description / comments:**

<!-- Add description/comments here. -->

Called when an attack action starts to set up variables and such.

### `PrepareDefense()`

**Access:** Public

```csharp
public void PrepareDefense()
```

**Description / comments:**

<!-- Add description/comments here. -->

Same as prepareattack

### `PrepareMovement()`

**Access:** Public

```csharp
public void PrepareMovement()
```

**Description / comments:**

<!-- Add description/comments here. -->

Same as prepareattack

### `ClearTiles()`

**Access:** Public

```csharp
public void ClearTiles()
```

**Description / comments:**

<!-- Add description/comments here. -->

Clears all tile effects and visuals. 

### `EvaluateDamage(Player.PlayerId playerId)`

**Access:** Public

```csharp
public async Task EvaluateDamage(Player.PlayerId playerId)
```

**Description / comments:**

Called at the end of at turn to start the damage sequence. The player passed into this function is the one receiving the damage.

<!-- Add description/comments here. -->

### `PruneUnitVisuals()`

**Access:** Public

```csharp
public void PruneUnitVisuals()
```

**Description / comments:**

<!-- Add description/comments here. -->

Removes unit visuals if the corresponding units health is 0. This does not remove it from the Units list however, it only deletes the visual. Recommended usage is calling this during each damage evaluation to ensure that the visual dissappears instanty if it takes lethal damage.

### `PruneUnitList()`

**Access:** Public

```csharp
public void PruneUnitList()
```

**Description / comments:**

<!-- Add description/comments here. -->

This is called at the end of the damage evaluation to clean up any Unit data entries that have health less than 0. We do this in bulk at the end for performance reasons.

### `GetCardAmount(Player.PlayerId id)`

**Access:** Public

```csharp
public int GetCardAmount(Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

Gets how many cards a player has placed.

### `AddUnit(Unit unit)`

**Access:** Public

```csharp
public bool AddUnit(Unit unit)
```

**Description / comments:**

<!-- Add description/comments here. -->

Adds the unit struct you pass in into the Unit list array.

### `IndexOfUnit(Unit unit)`

**Access:** Public

```csharp
public int IndexOfUnit(Unit unit)
```

**Description / comments:**

<!-- Add description/comments here. -->

Gets the index of the unit you pass in if it is in the unit list.

### `RemoveUnit(Unit unit)`

**Access:** Public

```csharp
public void RemoveUnit(Unit unit)
```

**Description / comments:**

<!-- Add description/comments here. -->

Removes the unit you pass in from the Unit list if it exists.

### `RemoveUnitAt(int index)`

**Access:** Public

```csharp
public void RemoveUnitAt(int index)
```

**Description / comments:**

<!-- Add description/comments here. -->

Self explanatory tbh

### `CoordinatesOf<T>(T[,] matrix, T value)`

**Access:** Public

```csharp
public Vector2Int CoordinatesOf<T>(T[,] matrix, T value)
```

**Description / comments:**

<!-- Add description/comments here. -->

Gets the cooridantes of a tile. Pass in TileVisual gameobject.

### `TileSelect()`

**Access:** Private

```csharp
private void TileSelect()
```

**Description / comments:**

<!-- Add description/comments here. -->

It selects a tile!

### `Attacking()`

**Access:** Private

```csharp
private void Attacking()
```

**Description / comments:**

<!-- Add description/comments here. -->

Run every frame while an attack is being placed.

### `Moving()`

**Access:** Private

```csharp
private void Moving()
```

**Description / comments:**

<!-- Add description/comments here. -->

Run every frame while a unit is being moved.

### `CardDataFromUnit(Unit unit)`

**Access:** Private

```csharp
private CardDeck.CardData CardDataFromUnit(Unit unit)
```

**Description / comments:**

<!-- Add description/comments here. -->

Used to get a CardData struct back from a unit struct. Used for certain things (i dont know which tbh)

### `BoardTakeDamage(int damage, Player.PlayerId id)`

**Access:** Private

```csharp
private void BoardTakeDamage(int damage, Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

Da board take da damage.

### `AddDamageInstance(DamageInstance instance)`

**Access:** Private

```csharp
private bool AddDamageInstance(DamageInstance instance)
```

**Description / comments:**

<!-- Add description/comments here. -->

This isn't usually called directly, you should use either the Local or Rpc version, depending if you want it on local player or other player.

### `AddDefenseInstance(DefenseInstance instance)`

**Access:** Private

```csharp
private bool AddDefenseInstance(DefenseInstance instance)
```

**Description / comments:**

<!-- Add description/comments here. -->

Same as above.

### `RemoveDamageInstanceAt(int index)`

**Access:** Private

```csharp
private void RemoveDamageInstanceAt(int index)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RemoveDefenseInstanceAt(int index)`

**Access:** Private

```csharp
private void RemoveDefenseInstanceAt(int index)
```

**Description / comments:**

<!-- Add description/comments here. -->

Self Explanatory

### `RemoveDamageInstancesForPlayer(Player.PlayerId id)`

**Access:** Private

```csharp
private void RemoveDamageInstancesForPlayer(Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

Self Explanatory

### `RemoveDefenseInstancesForPlayer(Player.PlayerId id)`

**Access:** Private

```csharp
private void RemoveDefenseInstancesForPlayer(Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `GetAdjacentTiles(...)`

**Access:** Private

```csharp
private static Vector2Int[] GetAdjacentTiles(
    Vector2Int position,
    IReadOnlyList<Unit> units,
    int unitCount,
    Player.PlayerId playerId)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `IsTileInBounds(Vector2Int position)`

**Access:** Private

```csharp
private static bool IsTileInBounds(Vector2Int position)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `HasUnitAt(...)`

**Access:** Private

```csharp
private static bool HasUnitAt(
    Vector2Int position,
    IReadOnlyList<Unit> units,
    int unitCount,
    Player.PlayerId playerId)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `UpdateBoardTileVisuals(...)`

**Access:** Private

```csharp
private static void UpdateBoardTileVisuals(
    PlayerBoard board,
    Player.PlayerId damageOwner,
    Player.PlayerId defenseOwner,
    IReadOnlyList<DamageInstance> damageInstances,
    int damageInstanceCount,
    IReadOnlyList<DefenseInstance> defenseInstances,
    int defenseInstanceCount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CalculateTotalDamage(...)`

**Access:** Private

```csharp
private static TotalDamage CalculateTotalDamage(
    Vector2Int position,
    Player.PlayerId damageOwner,
    Player.PlayerId defenseOwner,
    IReadOnlyList<DamageInstance> damageInstances,
    int damageInstanceCount,
    IReadOnlyList<DefenseInstance> defenseInstances,
    int defenseInstanceCount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `FindCoordinates<T>(T[,] matrix, T value)`

**Access:** Private

```csharp
private static Vector2Int FindCoordinates<T>(T[,] matrix, T value)
```

**Description / comments:**

<!-- Add description/comments here. -->


## BoardManager.PlayerBoard

Source: `Assets/Scripts/BoardManager.Models.cs`

### `PlayerBoard(GameObject[,] tileTransforms, GameObject[,] visuals)`

**Access:** Public

```csharp
public PlayerBoard(GameObject[,] tileTransforms, GameObject[,] visuals)
```

**Description / comments:**

<!-- Add description/comments here. -->


## BoardManager.DamageInstance

Source: `Assets/Scripts/BoardManager.Models.cs`

### `DamageInstance(string name, Player.PlayerId id, int damage, IList<Vector2Int> positions)`

**Access:** Public

```csharp
public DamageInstance(string name, Player.PlayerId id, int damage, IList<Vector2Int> positions)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ContainsPosition(Vector2Int position)`

**Access:** Public

```csharp
public bool ContainsPosition(Vector2Int position)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `NetworkSerialize<T>(BufferSerializer<T> serializer)`

**Access:** Public

```csharp
public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
```

**Description / comments:**

<!-- Add description/comments here. -->


## BoardManager.DefenseInstance

Source: `Assets/Scripts/BoardManager.Models.cs`

### `DefenseInstance(string name, Player.PlayerId id, int defense, IList<Vector2Int> positions)`

**Access:** Public

```csharp
public DefenseInstance(string name, Player.PlayerId id, int defense, IList<Vector2Int> positions)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ContainsPosition(Vector2Int position)`

**Access:** Public

```csharp
public bool ContainsPosition(Vector2Int position)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `NetworkSerialize<T>(BufferSerializer<T> serializer)`

**Access:** Public

```csharp
public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
```

**Description / comments:**

<!-- Add description/comments here. -->


## BoardManager.Unit

Source: `Assets/Scripts/BoardManager.Models.cs`

### `Unit(...)`

**Access:** Public

```csharp
public Unit(
    string name,
    int cardID,
    int cost,
    Player.PlayerId id,
    int health,
    int damage,
    int movement,
    IList<Vector2Int> attackPositions,
    Vector2Int position,
    int defense,
    bool hasActed)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `NetworkSerialize<T>(BufferSerializer<T> serializer)`

**Access:** Public

```csharp
public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
```

**Description / comments:**

<!-- Add description/comments here. -->


## CameraMainMenu

Source: `Assets/Scripts/CameraMainMenu.cs`

### `SetCameraState(int state)`

**Access:** Public

```csharp
public void SetCameraState(int state)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `MoveCamera(Vector3 targetPos, Quaternion targetRot)`

**Access:** Private

```csharp
private IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
```

**Description / comments:**

<!-- Add description/comments here. -->


## CardDeck

Source: `Assets/Scripts/CardDeck.cs`

### `FromJsonToDeck()`

**Access:** Public

```csharp
public void FromJsonToDeck()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `FromJSONToMaster()`

**Access:** Public

```csharp
public void FromJSONToMaster()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ClearDeck()`

**Access:** Public

```csharp
public void ClearDeck()
```

**Description / comments:**

<!-- Add description/comments here. -->


## CardManager

Source: `Assets/Scripts/CardManager.cs`

### `DrawCardHandler(int amount)`

**Access:** Public

```csharp
public void DrawCardHandler(int amount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RecallCard(GameObject cardVisual, BoardManager.Unit unit)`

**Access:** Public

```csharp
public void RecallCard(GameObject cardVisual, BoardManager.Unit unit)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RemoveCard(GameObject cardVisual)`

**Access:** Public

```csharp
public void RemoveCard(GameObject cardVisual)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `BuildCard(CardDeck.CardData cardData)`

**Access:** Public

```csharp
public GameObject BuildCard(CardDeck.CardData cardData)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `DrawCard(int amount)`

**Access:** Private

```csharp
private IEnumerator DrawCard(int amount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RecallCardRpc(int unitIndex, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void RecallCardRpc(int unitIndex, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->


## ConnectionManager

Source: `Assets/Scripts/ConnectionManager.cs`

### `LeaveSessionAsync()`

**Access:** Public

```csharp
public async Task LeaveSessionAsync()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartOrJoin()`

**Access:** Private

```csharp
private void StartOrJoin()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onUsernameSet(string value)`

**Access:** Private

```csharp
private void onUsernameSet(string value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onSessionNameSet(string value)`

**Access:** Private

```csharp
private void onSessionNameSet(string value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnClientDisconnect(NetworkManager manager, ConnectionEventData connectionEventData)`

**Access:** Private

```csharp
private async void OnClientDisconnect(NetworkManager manager, ConnectionEventData connectionEventData)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `WaitForShutdown()`

**Access:** Private

```csharp
private async Task WaitForShutdown()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ClearSessionState()`

**Access:** Private

```csharp
private void ClearSessionState()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnSessionOwnerPromoted(ulong sessionOwnerPromoted)`

**Access:** Private

```csharp
private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnClientConnectedCallback(ulong clientId)`

**Access:** Private

```csharp
private void OnClientConnectedCallback(ulong clientId)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CreateOrJoinSessionAsync()`

**Access:** Private

```csharp
private async Task CreateOrJoinSessionAsync()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnTransportFailure()`

**Access:** Private

```csharp
private void OnTransportFailure()
```

**Description / comments:**

<!-- Add description/comments here. -->


## GameManager

Source: `Assets/Scripts/GameManager.cs`

### `DisconnectUser()`

**Access:** Public

```csharp
public async void DisconnectUser()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `GetPlayerName()`

**Access:** Private

```csharp
private async void GetPlayerName()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SetPlayer1NameRpc(string name, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void SetPlayer1NameRpc(string name, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SetPlayer2NameRpc(string name, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void SetPlayer2NameRpc(string name, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->


## Hourglass

Source: `Assets/Scripts/Hourglass.cs`

### `AssignPositions(Transform leftPos, Transform rightPos)`

**Access:** Public

```csharp
public void AssignPositions(Transform leftPos, Transform rightPos)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `FlipHourglass(float timerLength)`

**Access:** Public

```csharp
public void FlipHourglass(float timerLength)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `InitiateTimer(float timerLength)`

**Access:** Private

```csharp
private IEnumerator InitiateTimer(float timerLength)
```

**Description / comments:**

<!-- Add description/comments here. -->


## InspectorButtonAttribute

Source: `Assets/Scripts/InspectorButton.cs`

### `InspectorButtonAttribute(string MethodName)`

**Access:** Public

```csharp
public InspectorButtonAttribute(string MethodName)
```

**Description / comments:**

<!-- Add description/comments here. -->


## Lobby

Source: `Assets/Scripts/Lobby.cs`

### `QuerySessions()`

**Access:** Public

```csharp
public async Task QuerySessions()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `LeaveSessionAsync()`

**Access:** Public

```csharp
public async Task LeaveSessionAsync()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `JoinSessionAsync(string id)`

**Access:** Public

```csharp
public async Task JoinSessionAsync(string id)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `JoinSessionByJoinCodeAsync(string joinCode)`

**Access:** Public

```csharp
public async Task JoinSessionByJoinCodeAsync(string joinCode)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CheckReconnect()`

**Access:** Private

```csharp
private void CheckReconnect()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `Reconnect()`

**Access:** Private

```csharp
private async void Reconnect()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartSession()`

**Access:** Private

```csharp
private void StartSession()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `JoinGameByJoinCode()`

**Access:** Private

```csharp
private void JoinGameByJoinCode()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onUsernameSet(string value)`

**Access:** Private

```csharp
private void onUsernameSet(string value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onSessionNameSet(string value)`

**Access:** Private

```csharp
private void onSessionNameSet(string value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onJoinCodeSet(string value)`

**Access:** Private

```csharp
private void onJoinCodeSet(string value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `onPrivateSet(bool value)`

**Access:** Private

```csharp
private void onPrivateSet(bool value)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnClientDisconnect(NetworkManager manager, ConnectionEventData connectionEventData)`

**Access:** Private

```csharp
private async void OnClientDisconnect(NetworkManager manager, ConnectionEventData connectionEventData)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `WaitForShutdown()`

**Access:** Private

```csharp
private async Task WaitForShutdown()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ClearSessionState()`

**Access:** Private

```csharp
private void ClearSessionState()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnSessionOwnerPromoted(ulong sessionOwnerPromoted)`

**Access:** Private

```csharp
private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CreateSessionAsync()`

**Access:** Private

```csharp
private async Task CreateSessionAsync()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnTransportFailure()`

**Access:** Private

```csharp
private void OnTransportFailure()
```

**Description / comments:**

<!-- Add description/comments here. -->


## ManaManager

Source: `Assets/Scripts/TacticsManager.cs`

### `AddManaPoints(int amount)`

**Access:** Public

```csharp
public void AddManaPoints(int amount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RemoveManaPoints(int amount)`

**Access:** Public

```csharp
public void RemoveManaPoints(int amount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CanAfford(int amount)`

**Access:** Public

```csharp
public bool CanAfford(int amount)
```

**Description / comments:**

<!-- Add description/comments here. -->


## OrbitCamera

Source: `Assets/Scripts/OrbitCamera.cs`

### `SwapCameraMode()`

**Access:** Public

```csharp
public void SwapCameraMode()
```

**Description / comments:**

<!-- Add description/comments here. -->


## Player

Source: `Assets/Scripts/Player.cs`

### `AssignPlayerID()`

**Access:** Public

```csharp
public static PlayerId AssignPlayerID()
```

**Description / comments:**

<!-- Add description/comments here. -->


## SessionInfoDisplay

Source: `Assets/Scripts/SessionInfoDisplay.cs`

### `SetSessionName(string name)`

**Access:** Public

```csharp
public void SetSessionName(string name)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SetJoinButton(string sessionID, Lobby manager)`

**Access:** Public

```csharp
public void SetJoinButton(string sessionID, Lobby manager)
```

**Description / comments:**

<!-- Add description/comments here. -->


## Settings

Source: `Assets/Scripts/Settings.cs`

### `toggleTutorial(bool on)`

**Access:** Public

```csharp
public void toggleTutorial(bool on)
```

**Description / comments:**

<!-- Add description/comments here. -->


## TextDialogue

Source: `Assets/Scripts/TextDialogue.cs`

### `DialogueRecieveStatus(int code)`

**Access:** Public

```csharp
public void DialogueRecieveStatus(int code)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `WaitACoupleSecs()`

**Access:** Private

```csharp
private IEnumerator WaitACoupleSecs()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `FadeAway()`

**Access:** Private

```csharp
private IEnumerator FadeAway()
```

**Description / comments:**

<!-- Add description/comments here. -->


## TurnManager

Source: `Assets/Scripts/TurnManager.cs`

### `ForceEndTurn()`

**Access:** Public

```csharp
public void ForceEndTurn()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `UpdateTurnText(TurnState turnState)`

**Access:** Public

```csharp
public void UpdateTurnText(TurnState turnState)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnTurnChanged(TurnState current)`

**Access:** Public

```csharp
public async void OnTurnChanged(TurnState current)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ChangeTurn()`

**Access:** Public

```csharp
public void ChangeTurn()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SetFirstTurnRpc(TurnState turn, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void SetFirstTurnRpc(TurnState turn, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartActivePlayerTurn(Player.PlayerId activePlayer, BoardManager boardManager)`

**Access:** Private

```csharp
private void StartActivePlayerTurn(Player.PlayerId activePlayer, BoardManager boardManager)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ResetUnitsForTurn(Player.PlayerId activePlayer, BoardManager.Unit[] units, int unitCount)`

**Access:** Private

```csharp
private void ResetUnitsForTurn(Player.PlayerId activePlayer, BoardManager.Unit[] units, int unitCount)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ShowManaPopupsForOpenTiles(Player.PlayerId activePlayer, BoardManager boardManager)`

**Access:** Private

```csharp
private void ShowManaPopupsForOpenTiles(Player.PlayerId activePlayer, BoardManager boardManager)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ChangeTurnRpc(TurnState turn, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void ChangeTurnRpc(TurnState turn, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ChangeTurnLocal(TurnState turn)`

**Access:** Private

```csharp
private void ChangeTurnLocal(TurnState turn)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `ApplyTurnChange(TurnState turn)`

**Access:** Private

```csharp
private void ApplyTurnChange(TurnState turn)
```

**Description / comments:**

<!-- Add description/comments here. -->


## UIDialogueSlide

Source: `Assets/Scripts/UIDialogueSlide.cs`

### `SlideIn()`

**Access:** Public

```csharp
public void SlideIn()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SlideOut()`

**Access:** Public

```csharp
public void SlideOut()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SlideInMouse()`

**Access:** Public

```csharp
public void SlideInMouse()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SlideOutMouse()`

**Access:** Public

```csharp
public void SlideOutMouse()
```

**Description / comments:**

<!-- Add description/comments here. -->


## UIDialogueTutorial

Source: `Assets/Scripts/UIDialogueTutorial.cs`

### `FlipPage(bool forwards)`

**Access:** Public

```csharp
public void FlipPage(bool forwards)
```

**Description / comments:**

<!-- Add description/comments here. -->


## UIManager

Source: `Assets/Scripts/UIManager.cs`

### `CreateInfoPanel(Vector2Int position, Player.PlayerId playerId)`

**Access:** Public

```csharp
public void CreateInfoPanel(Vector2Int position, Player.PlayerId playerId)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CreateCardInfoPanel(Vector2Int position, Player.PlayerId playerId)`

**Access:** Public

```csharp
public void CreateCardInfoPanel(Vector2Int position, Player.PlayerId playerId)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `DestroyCurrentInfoInstance()`

**Access:** Public

```csharp
public void DestroyCurrentInfoInstance()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `EnableControlsText()`

**Access:** Public

```csharp
public void EnableControlsText()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `DisplayEndGameScreen(Player.PlayerId id)`

**Access:** Public

```csharp
public IEnumerator DisplayEndGameScreen(Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `DrawTextAddCost(bool show)`

**Access:** Public

```csharp
public void DrawTextAddCost(bool show)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `UpdateHealthDisplay(Player.PlayerId id)`

**Access:** Private

```csharp
private void UpdateHealthDisplay(Player.PlayerId id)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `UpdateCardAmountDisplay()`

**Access:** Private

```csharp
private void UpdateCardAmountDisplay()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `RotateGridShape(Transform[] children, List<Vector2Int> positions)`

**Access:** Private

```csharp
private IEnumerator RotateGridShape(Transform[] children, List<Vector2Int> positions)
```

**Description / comments:**

<!-- Add description/comments here. -->


## UIManagerMainMenu

Source: `Assets/Scripts/UIManagerMainMenu.cs`

### `SetMenuScreen(int newState)`

**Access:** Public

```csharp
public void SetMenuScreen(int newState)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `SetMenuLevel(int menuLevel)`

**Access:** Public

```csharp
public void SetMenuLevel(int menuLevel)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `QuitGame()`

**Access:** Public

```csharp
public void QuitGame()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `AudioParchment()`

**Access:** Public

```csharp
public void AudioParchment()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `AudioStone()`

**Access:** Public

```csharp
public void AudioStone()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `PlaySlideNextFrame(UIDialogueSlide script, bool slidingIn)`

**Access:** Private

```csharp
private IEnumerator PlaySlideNextFrame(UIDialogueSlide script, bool slidingIn)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartCredits()`

**Access:** Private

```csharp
private void StartCredits()
```

**Description / comments:**

<!-- Add description/comments here. -->


## UIPopupNumbers

Source: `Assets/Scripts/UIPopupNumbers.cs`

### `Setup(float amount, int type)`

**Access:** Public

```csharp
public void Setup(float amount, int type)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `Create(Vector3 position, Transform parent, float amount, int type)`

**Access:** Public

```csharp
public static UIPopupNumbers Create(Vector3 position, Transform parent, float amount, int type)
```

**Description / comments:**

<!-- Add description/comments here. -->


## Vector2IntConverter

Source: `Assets/Scripts/CardDeck.cs`

### `ReadJson(...)`

**Access:** Public

```csharp
public override Vector2Int ReadJson(
    JsonReader reader,
    Type objectType,
    Vector2Int existingValue,
    bool hasExistingValue,
    JsonSerializer serializer)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)`

**Access:** Public

```csharp
public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
```

**Description / comments:**

<!-- Add description/comments here. -->


## tileColour

Source: `Assets/Scripts/tileColor.cs`

### `TileRecieveSignal(int newState, bool preview)`

**Access:** Public

```csharp
public void TileRecieveSignal(int newState, bool preview)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `TileRecieveDamage(int Damage, int Defence)`

**Access:** Public

```csharp
public void TileRecieveDamage(int Damage, int Defence)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `TileRecievePopup(int amount, int type)`

**Access:** Public

```csharp
public void TileRecievePopup(int amount, int type)
```

**Description / comments:**

<!-- Add description/comments here. -->


## tileFleshy

Source: `Assets/Scripts/tileFleshy.cs`

### `StartSinglePulse(float waitInMiliseconds)`

**Access:** Public

```csharp
public void StartSinglePulse(float waitInMiliseconds)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `Pulse(float waitInSeconds)`

**Access:** Public

```csharp
public IEnumerator Pulse(float waitInSeconds)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `CalcNoise()`

**Access:** Private

```csharp
private void CalcNoise()
```

**Description / comments:**

<!-- Add description/comments here. -->


## BoardManager.TotalDamage

Source: `Assets/Scripts/BoardManager.TileHelpers.cs`

### `TotalDamage(int incomingDamage, int defense, bool hasAttackOrDefense)`

**Access:** Public

```csharp
public TotalDamage(int incomingDamage, int defense, bool hasAttackOrDefense)
```

**Description / comments:**

<!-- Add description/comments here. -->


## CameraFollow

Source: `Assets/Scripts/CameraFollow.cs`

### `SendPositionRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void SendPositionRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->


## CardDrag

Source: `Assets/Scripts/CardDrag.cs`

### `GetMouseWorldPos()`

**Access:** Private

```csharp
private Vector3 GetMouseWorldPos()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `PlaceFailed(int error)`

**Access:** Private

```csharp
private void PlaceFailed(int error)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `GetCost()`

**Access:** Private

```csharp
private int GetCost()
```

**Description / comments:**

<!-- Add description/comments here. -->


## WaitingRoom

Source: `Assets/Scripts/WaitingRoom.cs`

### `UpdatePlayerList()`

**Access:** Private

```csharp
private void UpdatePlayerList()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `OnClientConnectedCallback(ulong id)`

**Access:** Private

```csharp
private void OnClientConnectedCallback(ulong id)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartGame()`

**Access:** Private

```csharp
private void StartGame()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartGameHandlerRpc(RpcParams rpcParams = default)`

**Access:** Private

```csharp
private void StartGameHandlerRpc(RpcParams rpcParams = default)
```

**Description / comments:**

<!-- Add description/comments here. -->

### `StartGameRoutine()`

**Access:** Private

```csharp
private IEnumerator StartGameRoutine()
```

**Description / comments:**

<!-- Add description/comments here. -->

### `LeaveGame()`

**Access:** Private

```csharp
private void LeaveGame()
```

**Description / comments:**

<!-- Add description/comments here. -->
