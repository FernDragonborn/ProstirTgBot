# ProstirTgGame
A simple telegram bot game for coliving I live in right now

Command for restoring SQL Server DB with command line:
sqlcmd -S localhost -U SA -Q "RESTORE DATABASE [ProtirTgBotDb] FROM DISK = '[path to .bak file]' WITH MOVE 'ProstirTgBotDb' TO '[path to .mdf file on server (will be stored there)]', MOVE 'ProstirTgBotDb_log' TO '[path to .ldf file on server (will be stored there)]', REPLACE, RECOVERY;" -C
Example:
sqlcmd -S localhost -U SA -Q "RESTORE DATABASE [ProtirTgBotDb] FROM DISK = '/home/fern_dragonborn/ProstirTgBotDb.bak' WITH MOVE 'ProstirTgBotDb' TO '/home/fern_dragonborn/ProstirTgBotDb.mdf', MOVE 'ProstirTgBotDb_log' TO '/home/fern_dragonborn/ProstirTgBotDb_log.ldf', REPLACE, RECOVERY;" -C
