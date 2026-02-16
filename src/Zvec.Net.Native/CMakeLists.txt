cmake_minimum_required(VERSION 3.16)
project(zvec_native VERSION 0.1.0 LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Build options
option(ZVEC_BUILD_SHARED "Build shared library" ON)
option(ZVEC_BUILD_STATIC "Build static library" OFF)

# Required paths
set(ZVEC_SRC_DIR "" CACHE PATH "Path to zvec source directory")
set(ZVEC_BUILD_DIR "" CACHE PATH "Path to zvec build directory")

# Defaults
if(NOT ZVEC_SRC_DIR)
    set(ZVEC_SRC_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../zvec")
endif()
if(NOT ZVEC_BUILD_DIR)
    set(ZVEC_BUILD_DIR "${CMAKE_BINARY_DIR}/../zvec")
endif()

get_filename_component(ZVEC_SRC_DIR "${ZVEC_SRC_DIR}" ABSOLUTE)
get_filename_component(ZVEC_BUILD_DIR "${ZVEC_BUILD_DIR}" ABSOLUTE)

message(STATUS "ZVEC_SRC_DIR: ${ZVEC_SRC_DIR}")
message(STATUS "ZVEC_BUILD_DIR: ${ZVEC_BUILD_DIR}")

# Verify paths
if(NOT EXISTS "${ZVEC_SRC_DIR}/src/include/zvec/db/collection.h")
    message(FATAL_ERROR "zvec headers not found at ${ZVEC_SRC_DIR}/src/include")
endif()

# zvec include directories
set(ZVEC_INCLUDE_DIRS
    "${ZVEC_SRC_DIR}/src/include"
    "${ZVEC_SRC_DIR}/src"
    "${ZVEC_BUILD_DIR}"  # For generated protobuf files
)

# zvec libraries (from build directory)
# These are created by zvec's CMake build
# The zvec build produces libraries like libzvec_db.a, libzvec_core.a, etc.
set(ZVEC_LIBRARIES
    "${ZVEC_BUILD_DIR}/lib/libzvec_db.a"
    "${ZVEC_BUILD_DIR}/lib/libzvec_core.a"
    "${ZVEC_BUILD_DIR}/lib/libzvec_ailego.a"
)

# Third-party libraries that zvec depends on
# These are built by zvec's thirdparty/
file(GLOB ZVEC_THIRDPARTY_LIBS 
    "${ZVEC_BUILD_DIR}/external/usr/local/lib/*.a"
    "${ZVEC_BUILD_DIR}/thirdparty/protobuf/protobuf-3.21.12/libprotobuf.a"
    "${ZVEC_BUILD_DIR}/thirdparty/rocksdb/rocksdb-8.1.1/librocksdb.a"
    "${ZVEC_BUILD_DIR}/thirdparty/antlr/antlr4/runtime/Cpp/libantlr4_static.a"
)

# Native library sources
set(ZVEC_NATIVE_SOURCES
    zvec_c.cc
)

# Create the native library
if(ZVEC_BUILD_SHARED)
    add_library(zvec_native SHARED ${ZVEC_NATIVE_SOURCES})
elseif(ZVEC_BUILD_STATIC)
    add_library(zvec_native STATIC ${ZVEC_NATIVE_SOURCES})
endif()

target_include_directories(zvec_native
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}
        ${ZVEC_INCLUDE_DIRS}
)

# Link with zvec libraries - use whole-archive to ensure all symbols are available
if(EXISTS "${ZVEC_BUILD_DIR}/lib/libzvec_db.a")
    message(STATUS "Linking with zvec libraries from build directory")
    
    # Collect all .a files
    file(GLOB_RECURSE ALL_STATIC_LIBS "${ZVEC_BUILD_DIR}/*.a")
    
    # Create a list of libraries to link
    set(LINK_LIBS "")
    
    # zvec core libraries (with libzvec_ prefix)
    foreach(lib zvec_db zvec_core zvec_ailego zvec_index zvec_proto zvec_common zvec_sqlengine)
        if(EXISTS "${ZVEC_BUILD_DIR}/lib/lib${lib}.a")
            list(APPEND LINK_LIBS "${ZVEC_BUILD_DIR}/lib/lib${lib}.a")
            message(STATUS "  Found: lib${lib}.a")
        endif()
    endforeach()
    
    # Third-party libraries (built by zvec)
    foreach(lib 
        protobuf
        rocksdb
        antlr4_static
        roaring
        glog
        gflags_nothreads_static
        yaml-cpp
        lz4_static
    )
        file(GLOB FOUND_LIB "${ZVEC_BUILD_DIR}/*${lib}*.a")
        if(FOUND_LIB)
            list(APPEND LINK_LIBS ${FOUND_LIB})
            message(STATUS "  Found: ${FOUND_LIB}")
        endif()
    endforeach()
    
    # Arrow libraries
    file(GLOB ARROW_LIBS "${ZVEC_BUILD_DIR}/external/usr/local/lib/libArrow*.a")
    if(ARROW_LIBS)
        list(APPEND LINK_LIBS ${ARROW_LIBS})
        message(STATUS "  Found Arrow: ${ARROW_LIBS}")
    endif()
    
    target_link_libraries(zvec_native
        PRIVATE
            ${LINK_LIBS}
            pthread
            dl
    )
else()
    message(STATUS "zvec libraries not found - using stub mode")
    target_link_libraries(zvec_native PRIVATE pthread dl)
endif()

# Set output directory
set_target_properties(zvec_native PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
)

# Platform-specific settings
if(APPLE)
    find_library(FOUNDATION_FRAMEWORK Foundation)
    if(FOUNDATION_FRAMEWORK)
        target_link_libraries(zvec_native PRIVATE ${FOUNDATION_FRAMEWORK})
    endif()
    set_target_properties(zvec_native PROPERTIES
        INSTALL_NAME_DIR "@rpath"
        MACOSX_RPATH ON
    )
endif()

# Install rules
install(TARGETS zvec_native
    LIBRARY DESTINATION lib
)
install(FILES zvec_c.h DESTINATION include)
