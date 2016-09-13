# Bot pull request rules

Bot pull request should contain new bot only. 
In particular, the following set of rules exist:

* Pull request MUST NOT change any code outside `TicTacToeGame.Players`    
* For bot of name EXAMPLE several objects MUST be added:  
    * Folder `TicTacToeGame.Players\EXAMPLE` containing all bot code  
    * Class `TicTacToeGame.Players.EXAMPLE.EXAMPLEBot` which MUST inherit `TicTacToeGame.Players.BotPlayer`  
    * Enum element `TicTacToeGame.Players.Enums.BotKind.EXAMPLE`  
    * `case` in `TicTacToeGame.Players.BotFactory` which simply returns class instanse  
* Namespaces MUST be aligned to folders
* No code changes in `TicTacToeGame.Players` except described above are allowed  