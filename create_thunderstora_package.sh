#!/usr/bin/env bash

set -euxo pipefail

dotnet build -c Release
7z a ChallengerPEAK.zip icon.png manifest.json README.md CHANGELOG.md ./bin/Release/netstandard2.1/com.github.raspberry1111.challengerpeak.dll