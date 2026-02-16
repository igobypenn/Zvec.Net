#!/bin/bash
# Build script for zvec-net native library
# Builds zvec first, then builds the native C wrapper
#
# Usage: ./build-native.sh [Release|Debug]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Configuration
BUILD_TYPE="${1:-Release}"
ZVEC_SRC="${SCRIPT_DIR}/zvec"
NPROC=$(nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 4)

# Detect platform
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)
case "$ARCH" in
    x86_64|amd64) ARCH="x64" ;;
    aarch64|arm64) ARCH="arm64" ;;
esac
RUNTIME_ID="${OS}-${ARCH}"

echo "========================================"
echo "Building zvec-net native library"
echo "========================================"
echo "Build type: $BUILD_TYPE"
echo "zvec source: $ZVEC_SRC"
echo "Platform:  $RUNTIME_ID"
echo "Jobs:      $NPROC"
echo "========================================"

# Verify zvec exists
if [ ! -f "$ZVEC_SRC/CMakeLists.txt" ]; then
    echo "ERROR: zvec not found at $ZVEC_SRC"
    echo ""
    echo "Please initialize the submodule:"
    echo "  git submodule update --init --recursive"
    exit 1
fi

# ============================================
# Stage 1: Build zvec
# ============================================
ZVEC_BUILD_DIR="$SCRIPT_DIR/build/zvec"

echo ""
echo ">>> Stage 1: Building zvec..."
echo "    Build dir: $ZVEC_BUILD_DIR"

mkdir -p "$ZVEC_BUILD_DIR"

# Configure zvec
cmake -B "$ZVEC_BUILD_DIR" -S "$ZVEC_SRC" \
    -DCMAKE_BUILD_TYPE="$BUILD_TYPE" \
    -DBUILD_PYTHON_BINDINGS=OFF \
    -DBUILD_TOOLS=OFF \
    -DCMAKE_POSITION_INDEPENDENT_CODE=ON \
    -DCMAKE_POLICY_VERSION_MINIMUM=3.5

# Build zvec
cmake --build "$ZVEC_BUILD_DIR" --config "$BUILD_TYPE" -j"$NPROC" -v

# List the built zvec libraries
echo ""
echo ">>> Built zvec libraries:"
find "$ZVEC_BUILD_DIR" -name "libzvec*.a" -o -name "libailego*.a" 2>/dev/null | head -20

# ============================================
# Stage 2: Build native wrapper using zvec
# ============================================
NATIVE_BUILD_DIR="$SCRIPT_DIR/build/native"

echo ""
echo ">>> Stage 2: Building native wrapper..."
echo "    Build dir: $NATIVE_BUILD_DIR"

mkdir -p "$NATIVE_BUILD_DIR"

# Configure with zvec paths
cmake -B "$NATIVE_BUILD_DIR" -S src/Zvec.Net.Native \
    -DZVEC_SRC_DIR="$ZVEC_SRC" \
    -DZVEC_BUILD_DIR="$ZVEC_BUILD_DIR" \
    -DCMAKE_BUILD_TYPE="$BUILD_TYPE"

# Build
cmake --build "$NATIVE_BUILD_DIR" --config "$BUILD_TYPE" -j"$NPROC" -v

# Prepare output directory
OUTPUT_DIR="$SCRIPT_DIR/src/Zvec.Net/runtimes/$RUNTIME_ID/native"
mkdir -p "$OUTPUT_DIR"

# Copy library
echo ""
echo ">>> Copying library to $OUTPUT_DIR..."
if [ -f "$NATIVE_BUILD_DIR/lib/libzvec_native.so" ]; then
    cp "$NATIVE_BUILD_DIR/lib/libzvec_native.so" "$OUTPUT_DIR/"
elif [ -f "$NATIVE_BUILD_DIR/lib/libzvec_native.dylib" ]; then
    cp "$NATIVE_BUILD_DIR/lib/libzvec_native.dylib" "$OUTPUT_DIR/"
else
    echo "ERROR: Built library not found in $NATIVE_BUILD_DIR/lib/"
    ls -la "$NATIVE_BUILD_DIR/lib/" 2>/dev/null || echo "Directory does not exist"
    exit 1
fi

echo ""
echo "========================================"
echo "Build successful!"
echo "Library: $OUTPUT_DIR"
ls -lh "$OUTPUT_DIR"
echo "========================================"
