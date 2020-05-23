rem *********************************
REM -- Recup de l'emplacement du fichier
SET cdir=%~dp0
SET cdir=%cdir:~0,-1%

SET STEAM_CMD_FOLDER=C:\Steam\SteamCmd

%STEAM_CMD_FOLDER%\steamcmd +login helfima +workshop_build_item %cdir%\publish_preview.vdf +quit
pause