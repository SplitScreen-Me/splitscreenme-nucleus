#!/bin/bash

#usage: sh find_interfaces.sh "path to original steam api .dll or .so" > steam_interfaces.txt

#created by supergoat from cs.rin.ru
#modified by Mr_Goldberg

fi=$1

function findinterface {
strings "$fi" | grep -P "$1\d{3}"
}

findinterface SteamClient
findinterface SteamGameServer
findinterface SteamGameServerStats
findinterface SteamUser
findinterface SteamFriends
findinterface SteamUtils
findinterface SteamMatchMaking
findinterface SteamMatchMakingServers
findinterface STEAMUSERSTATS_INTERFACE_VERSION
findinterface STEAMAPPS_INTERFACE_VERSION
findinterface SteamNetworking
findinterface STEAMREMOTESTORAGE_INTERFACE_VERSION
findinterface STEAMSCREENSHOTS_INTERFACE_VERSION
findinterface STEAMHTTP_INTERFACE_VERSION
findinterface STEAMUNIFIEDMESSAGES_INTERFACE_VERSION
findinterface STEAMCONTROLLER_INTERFACE_VERSION
findinterface STEAMUGC_INTERFACE_VERSION
findinterface STEAMAPPLIST_INTERFACE_VERSION
findinterface STEAMMUSIC_INTERFACE_VERSION
findinterface STEAMMUSICREMOTE_INTERFACE_VERSION
findinterface STEAMHTMLSURFACE_INTERFACE_VERSION_
findinterface STEAMINVENTORY_INTERFACE_V
findinterface SteamController
