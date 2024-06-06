-- Manifest data
fx_version 'bodacious'
games {'gta5'}

-- Resource stuff
name 'ScenarioCreator'
description ''
version 'v0.1.0'
author 'klandestino'
ui_page 'storage.html'

-- Adds additional logging, useful when debugging issues.
client_debug_mode 'false'
server_debug_mode 'false'

-- Leave this set to '0' to prevent compatibility issues 
-- and to keep the save files your users.
experimental_features_enabled '0'

-- Files & scripts
files {
    'Newtonsoft.Json.dll',
    'MenuAPI.dll',
    'config/locations.json',
    'config/addons.json',
    'storage.html'
}

client_script 'ScenarioCreatorClient.net.dll'
server_script 'ScenarioCreatorServer.net.dll'
