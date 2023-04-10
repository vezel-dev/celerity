#!/usr/bin/env bash
set -eou pipefail

dotnet cake celerity.cake $@
