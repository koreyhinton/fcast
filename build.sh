#!/bin/bash
set -e

if [[ -z "$NSMAP" ]]; then
    echo "Requires orc NSMAP var to be set" 1>&2
    echo "run with path to nsmap dir specified" 1>&2
    exit 1
fi
if [[ ! -d "$NSMAP" ]]; then
    echo "NSMAP dir not found: $NSMAP" 1>&2
    exit 1
fi

# Generate into a .tmp (Unity ignores *.tmp) and only replace the .cs on
# success, so a failed run never leaves a half-written file.
(
    cd ./Assets/Scripts/Fcast
    ./FcastMain.sh > ./FcastMain.cs.tmp || { rm -f ./FcastMain.cs.tmp; exit 1; }
    mv ./FcastMain.cs.tmp ./FcastMain.cs
)
