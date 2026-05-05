#!/bin/bash
# build.sh — UDEO C# Build Script
# Builds the entire UDEO.CSharp solution including C++ native components.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_CONFIG="${1:-Release}"

echo "============================================"
echo "  UDEO v3.1.0 — C# Build Pipeline"
echo "  Configuration: $BUILD_CONFIG"
echo "============================================"

# Step 1: Build C++ native library (optional, uses CMake)
NATIVE_DIR="$SCRIPT_DIR/src/UDEO.Native/Physics"
if command -v cmake &> /dev/null && [ -f "$NATIVE_DIR/CMakeLists.txt" ]; then
    echo ""
    echo "[1/3] Building C++ native library..."
    NATIVE_BUILD="$SCRIPT_DIR/build/native"
    mkdir -p "$NATIVE_BUILD"
    cmake -S "$NATIVE_DIR" -B "$NATIVE_BUILD" -DCMAKE_BUILD_TYPE="$BUILD_CONFIG"
    cmake --build "$NATIVE_BUILD" --config "$BUILD_CONFIG"
    echo "  Native library built successfully."
else
    echo "[1/3] Skipping C++ native library (CMake not found or no CMakeLists.txt)"
    echo "  Falling back to managed implementations."
fi

# Step 2: Restore NuGet packages
echo ""
echo "[2/3] Restoring NuGet packages..."
dotnet restore "$SCRIPT_DIR/UDEO.sln" --verbosity quiet

# Step 3: Build the .NET solution
echo ""
echo "[3/3] Building .NET solution..."
dotnet build "$SCRIPT_DIR/UDEO.sln" --configuration "$BUILD_CONFIG" --no-restore

echo ""
echo "============================================"
echo "  Build complete!"
echo "  Output: src/UDEO.Cli/bin/$BUILD_CONFIG/net8.0/"
echo "============================================"
