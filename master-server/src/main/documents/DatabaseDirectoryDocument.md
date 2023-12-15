```DbInterface Class```

DbInterface Class

    Purpose: This class serves as a gateway to our MongoDB database. It abstracts the logic for connecting to the database and accessing collections.
    Static vs Instance: Making it an instance class (non-static) is a good practice. It allows for easier unit testing and better control over the lifecycle of database connections.

PlayerManager Class

    Purpose: Manages CRUD operations for the Player collection. It uses the DbInterface to interact with the MongoDB database.
    Methods: You have methods for creating a player and retrieving a player by ID. Additional methods for updating and deleting players will make it a comprehensive manager for player data.

GameServerManager Class

    This class is used to manage data of type GameServer and performs operations specifically related to game servers.
    Code Functions:

    GameServer Collection: The GameServerManager class provides access to data of type GameServer via the DbInterface. This corresponds to the collection named "GameServers" in MongoDB.

    Add GameServer: The AddGameServer method adds a new GameServer object to the database. This is used to save the game server information.

LobbyManager Class

    Example Scenarios

    Lobby Creation: Players can create a lobby before starting a game. The CreateLobby method stores this lobby in the database.

    Lobby Update: When players are added to or leave the game lobby, the lobby information is updated.

    Lobby Query: When players want to view the available lobbies, LobbyManager can query the available lobbies.

    Lobby Delete: When a game ends or the lobby is no longer valid, the corresponding lobby can be deleted from the database.