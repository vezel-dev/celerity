#!/usr/bin/env bash
set -eou pipefail

dotnet tool restore
dotnet cake celerity.cake $@
