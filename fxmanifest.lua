-- Manifest data
fx_version 'bodacious'
games {'gta5'}

-- Resource stuff
name 'ScenarioCreator'
description ''
version 'v0.1.0'
author 'klandestino'

client_debug_mode 'false'
server_debug_mode 'false'

experimental_features_enabled '0'

-- Files & scripts
files {
    "System.Numerics.dll",
    'ScenarioCreator.net.deps.json',
    'ScenarioCreatorServer.net.deps.json',
}

client_script 'ScenarioCreator.net.dll'
server_script 'ScenarioCreatorServer.net.dll'
