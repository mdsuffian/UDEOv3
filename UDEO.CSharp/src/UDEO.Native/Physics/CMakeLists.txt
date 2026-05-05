cmake_minimum_required(VERSION 3.20)
project(UDEO.Native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Build as shared library
add_library(UDEO.Native SHARED
    PhysicsEngine.cpp
)

# Export define
target_compile_definitions(UDEO.Native PRIVATE UDEO_NATIVE_EXPORTS)

# Optimization flags
if(MSVC)
    target_compile_options(UDEO.Native PRIVATE /O2 /arch:AVX2)
else()
    target_compile_options(UDEO.Native PRIVATE -O3 -march=native)
endif()

# Output directory
set_target_properties(UDEO.Native PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
)
