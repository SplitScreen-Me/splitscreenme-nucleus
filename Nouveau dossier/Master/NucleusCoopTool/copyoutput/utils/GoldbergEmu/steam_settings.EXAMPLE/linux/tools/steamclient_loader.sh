#!/bin/bash
APP_NAME="bin/test_executable"
APP_ID=480
APP_PATH=$(dirname "$0")
CONFIG_PATH=$(dirname "$0")
#path to steam-runtime/run.sh
STEAM_RUNTIME=""

CUR_DIR=$(pwd)
cd "$CONFIG_PATH"
mkdir -p ~/.steam/sdk64
mkdir -p ~/.steam/sdk32
#make a backup of original files
mv ~/.steam/steam.pid ~/.steam/steam.pid.orig || true
mv ~/.steam/sdk64/steamclient.so ~/.steam/sdk64/steamclient.so.orig || true
mv ~/.steam/sdk32/steamclient.so ~/.steam/sdk32/steamclient.so.orig || true
#copy our files
cp x86/steamclient.so ~/.steam/sdk32/steamclient.so
cp x86_64/steamclient.so ~/.steam/sdk64/steamclient.so
echo $BASHPID > ~/.steam/steam.pid
cd "$APP_PATH"
if [ -z "$STEAM_RUNTIME" ]
then
SteamAppPath="$APP_PATH" SteamAppId=$APP_ID SteamGameId=$APP_ID "$APP_NAME"
else
SteamAppPath="$APP_PATH" SteamAppId=$APP_ID SteamGameId=$APP_ID "$STEAM_RUNTIME" "$APP_NAME"
fi

cd "$CUR_DIR"
#restore original
rm -f ~/.steam/steam.pid
rm -f ~/.steam/sdk64/steamclient.so
rm -f ~/.steam/sdk32/steamclient.so
mv ~/.steam/steam.pid.orig ~/.steam/steam.pid
mv ~/.steam/sdk64/steamclient.so.orig ~/.steam/sdk64/steamclient.so || true
mv ~/.steam/sdk32/steamclient.so.orig ~/.steam/sdk32/steamclient.so || true
