#!/bin/bash

##########
## Constants
##########
gamePath="/home/pathoschild/Stardew Valley"
bundleModNames=("ConsoleCommands" "ErrorHandler" "SaveBackup")
buildConfig="Release"
folders=("unix" "windows")
declare -A runtimes=(["unix"]="linux-x64" ["windows"]="win-x64")
declare -A msBuildPlatformNames=(["unix"]="Unix" ["windows"]="Windows_NT")


##########
## Move to SMAPI root
##########
cd "`dirname "$0"`/.."


##########
## Clear old build files
##########
echo "Clearing old builds..."
echo "-----------------------"
for path in */**/bin */**/obj; do
    rm -rf $path
done
rm -rf "bin"
echo ""

##########
## Compile files
##########
for folder in ${folders[@]}; do
    runtime=${runtimes[$folder]}
    msbuildPlatformName=${msBuildPlatformNames[$folder]}

    # SMAPI
    echo "Compiling SMAPI for $folder..."
    echo "------------------"
    dotnet publish src/SMAPI --configuration $buildConfig -v minimal --runtime "$runtime" -p:OS="$msbuildPlatformName" -p:GamePath="$gamePath" -p:CopyToGameFolder="false"
    echo ""
    echo ""

    echo "Compiling installer for $folder..."
    echo "----------------------"
    dotnet publish src/SMAPI.Installer --configuration $buildConfig -v minimal --runtime "$runtime" -p:OS="$msbuildPlatformName" -p:GamePath="$gamePath" -p:CopyToGameFolder="false"
    echo ""
    echo ""

    for modName in ${bundleModNames[@]}; do
        echo "Compiling $modName for $folder..."
        echo "----------------------------------"
        dotnet publish src/SMAPI.Mods.$modName --configuration $buildConfig -v minimal --runtime "$runtime" -p:OS="$msbuildPlatformName" -p:GamePath="$gamePath" -p:CopyToGameFolder="false"
        echo ""
        echo ""
    done
done


##########
## Prepare install package
##########
echo "Preparing install package..."
echo "----------------------------"

# init paths
installAssets="src/SMAPI.Installer/assets"
packagePath="bin/SMAPI installer"
packageDevPath="bin/SMAPI installer for developers"

# init structure
for folder in ${folders[@]}; do
    mkdir "$packagePath/internal/$folder/bundle/smapi-internal" --parents
done

# copy base installer files
cp "$installAssets/unix-install.sh"     "$packagePath/install on Linux.sh"
cp "$installAssets/unix-install.sh"     "$packagePath/install on macOS.command"
cp "$installAssets/windows-install.bat" "$packagePath/install on Windows.bat"
cp "$installAssets/README.txt"          "$packagePath/README.txt"

# copy per-platform files
for folder in ${folders[@]}; do
    runtime=${runtimes[$folder]}

    # get paths
    installBin="src/SMAPI.Installer/bin/$buildConfig/$runtime"
    smapiBin="src/SMAPI/bin/$buildConfig/$runtime"
    internalPath="$packagePath/internal/$folder"
    bundlePath="$internalPath/bundle"

    # runtime config for installer
    cp "$installBin/SMAPI.Installer.runtimeconfig.json" "$internalPath/SMAPI.Installer.runtimeconfig.json"

    # runtime config for SMAPI
    cp "$installAssets/runtimeconfig.$folder.json" "$bundlePath/StardewModdingAPI.runtimeconfig.json"

    # installer DLL
    cp "$installBin/SMAPI.Installer.dll" "$internalPath/SMAPI.Installer.dll"
    if [ $folder == "windows" ]; then
        cp "$installAssets/windows-exe-config.xml" "$packagePath/internal/windows/install.exe.config"
    fi

    # bundle root files
    for name in "StardewModdingAPI" "StardewModdingAPI.dll" "StardewModdingAPI.pdb" "StardewModdingAPI.xml" "steam_appid.txt"; do
        if [ $name == "StardewModdingAPI" ] && [ $folder == "windows" ]; then
            name="$name.exe"
        fi

        cp "$smapiBin/$name" "$bundlePath/$name"
    done

    # bundle i18n
    cp -r "$smapiBin/i18n" "$bundlePath/smapi-internal"

    # bundle smapi-internal
    for name in "0Harmony.dll" "0Harmony.xml" "Mono.Cecil.dll" "Mono.Cecil.Mdb.dll" "Mono.Cecil.Pdb.dll" "MonoMod.Common.dll" "Newtonsoft.Json.dll" "TMXTile.dll" "SMAPI.Toolkit.dll" "SMAPI.Toolkit.pdb" "SMAPI.Toolkit.xml" "SMAPI.Toolkit.CoreInterfaces.dll" "SMAPI.Toolkit.CoreInterfaces.pdb" "SMAPI.Toolkit.CoreInterfaces.xml"; do
        cp "$smapiBin/$name" "$bundlePath/smapi-internal/$name"
    done

    cp "$smapiBin/SMAPI.config.json" "$bundlePath/smapi-internal/config.json"
    cp "$smapiBin/SMAPI.metadata.json" "$bundlePath/smapi-internal/metadata.json"
    if [ $folder == "unix" ]; then
        cp "$installAssets/unix-launcher.sh" "$bundlePath/unix-launcher.sh"
        cp "$smapiBin/System.Runtime.Caching.dll" "$bundlePath/smapi-internal/System.Runtime.Caching.dll"
    else
        cp "$installAssets/windows-exe-config.xml" "$bundlePath/StardewModdingAPI.exe.config"
    fi

    # copy .NET dependencies
    cp "$smapiBin/System.Configuration.ConfigurationManager.dll" "$bundlePath/smapi-internal/System.Configuration.ConfigurationManager.dll"
    cp "$smapiBin/System.Runtime.Caching.dll" "$bundlePath/smapi-internal/System.Runtime.Caching.dll"
    cp "$smapiBin/System.Security.Permissions.dll" "$bundlePath/smapi-internal/System.Security.Permissions.dll"
    if [ $folder == "windows" ]; then
        cp "$smapiBin/System.Management.dll" "$bundlePath/smapi-internal/System.Management.dll"
    fi

    # copy bundled mods
    for modName in ${bundleModNames[@]}; do
        fromPath="src/SMAPI.Mods.$modName/bin/$buildConfig/$runtime"
        targetPath="$bundlePath/Mods/$modName"

        mkdir "$targetPath" --parents

        cp "$fromPath/$modName.dll" "$targetPath/$modName.dll"
        cp "$fromPath/$modName.pdb" "$targetPath/$modName.pdb"
        cp "$fromPath/manifest.json" "$targetPath/manifest.json"
        if [ -d "$fromPath/i18n" ]; then
            cp -r "$fromPath/i18n" "$targetPath"
        fi
    done
done

# mark scripts executable
for path in "install on Linux.sh" "install on macOS.command" "bundle/unix-launcher.sh"; do
    if [ -f "$packagePath/$path" ]; then
        chmod 755 "$packagePath/$path"
    fi
done

# split into main + for-dev folders
cp -r "$packagePath" "$packageDevPath"
for folder in ${folders[@]}; do
    # disable developer mode in main package
    sed --in-place --expression="s/\"DeveloperMode\": true/\"DeveloperMode\": false/" "$packagePath/internal/$folder/bundle/smapi-internal/config.json"

    # convert bundle folder into final 'install.dat' files
    for path in "$packagePath/internal/$folder" "$packageDevPath/internal/$folder"; do
        pushd "$path/bundle" > /dev/null
        zip "install.dat" * --recurse-paths --quiet
        popd > /dev/null
        mv "$path/bundle/install.dat" "$path/install.dat"
        rm -rf "$path/bundle"
    done
done


##########
## Create release zips
##########
# get version number
version="$1"
if [ $# -eq 0 ]; then
    echo "SMAPI release version (like '4.0.0'):"
    read version
fi

# rename folders
mv "$packagePath" "bin/SMAPI $version installer"
mv "$packageDevPath" "bin/SMAPI $version installer for developers"


# package files
pushd bin > /dev/null
zip -9 "SMAPI $version installer.zip" "SMAPI $version installer" --recurse-paths --quiet
zip -9 "SMAPI $version installer for developers.zip" "SMAPI $version installer for developers" --recurse-paths --quiet
popd > /dev/null

echo ""
echo "Done! Package created in $(pwd)/bin"
