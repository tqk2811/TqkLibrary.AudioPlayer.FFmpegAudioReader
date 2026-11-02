cmake_minimum_required(VERSION 3.16)
project(TqkLibraryAudioPlayerFFmpegAudioReaderNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

set(TARGET_NAME "TqkLibrary.AudioPlayer.FFmpegAudioReader.Native")

# ---------------------------------------------------------------------------
# FFmpeg inputs.
# These come from the TqkLibrary.FFmpeg.Gpl2.Native.<RID> NuGet packages and are
# supplied by the build script / CI:
#   FFMPEG_INCLUDE_DIR  -> <pkg>/build/native/include            (all platforms)
#   FFMPEG_LIB_DIR      -> <pkg>/build/native/win/<arch>/lib     (Windows only, import libs)
#   FFMPEG_RUNTIME_DIR  -> <pkg>/runtimes/<rid>/native           (Linux: the .so to link against)
# ---------------------------------------------------------------------------
if(NOT DEFINED FFMPEG_INCLUDE_DIR)
    message(FATAL_ERROR "FFMPEG_INCLUDE_DIR is not set (path to <pkg>/build/native/include)")
endif()
if(NOT EXISTS "${FFMPEG_INCLUDE_DIR}/libavformat/avformat.h")
    message(FATAL_ERROR "avformat.h not found under FFMPEG_INCLUDE_DIR=${FFMPEG_INCLUDE_DIR}")
endif()

# Version for the Windows version resource (defaults keep local builds working).
if(NOT DEFINED VER_FILE_MAJOR)
    set(VER_FILE_MAJOR 0)
endif()
if(NOT DEFINED VER_FILE_MINOR)
    set(VER_FILE_MINOR 0)
endif()
if(NOT DEFINED VER_FILE_BUILD)
    set(VER_FILE_BUILD 0)
endif()
configure_file(
    "${CMAKE_CURRENT_SOURCE_DIR}/version.generated.h.in"
    "${CMAKE_CURRENT_BINARY_DIR}/version.generated.h"
    @ONLY)

set(SOURCES
    Exports.cpp
    AudioSource.cpp)
if(WIN32)
    list(APPEND SOURCES dllmain.cpp version.rc)
endif()

add_library(${TARGET_NAME} SHARED ${SOURCES})
set_target_properties(${TARGET_NAME} PROPERTIES
    OUTPUT_NAME "${TARGET_NAME}"
    PREFIX ""            # avoid a 'lib' prefix juggling; on Linux we still get libXXX.so via SOVERSION-less naming below
    )

target_include_directories(${TARGET_NAME} PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}"
    "${CMAKE_CURRENT_BINARY_DIR}"   # version.generated.h
    "${FFMPEG_INCLUDE_DIR}")

if(WIN32)
    if(NOT DEFINED FFMPEG_LIB_DIR)
        message(FATAL_ERROR "FFMPEG_LIB_DIR is not set (path to <pkg>/build/native/win/<arch>/lib)")
    endif()
    foreach(lib avformat avcodec avutil)
        find_library(FFLIB_${lib} NAMES ${lib} PATHS "${FFMPEG_LIB_DIR}" NO_DEFAULT_PATH)
        if(NOT FFLIB_${lib})
            message(FATAL_ERROR "Import lib ${lib}.lib not found in FFMPEG_LIB_DIR=${FFMPEG_LIB_DIR}")
        endif()
        target_link_libraries(${TARGET_NAME} PRIVATE "${FFLIB_${lib}}")
    endforeach()
    if(MSVC)
        target_compile_options(${TARGET_NAME} PRIVATE /utf-8 /W3)
    endif()
else()
    # On Linux the package ships only the fully-versioned .so (libavcodec.so.62.x.y);
    # link against it directly. Runtime loading of these deps is handled by the
    # managed side (it pre-loads them by SONAME before loading this wrapper).
    if(NOT DEFINED FFMPEG_RUNTIME_DIR)
        message(FATAL_ERROR "FFMPEG_RUNTIME_DIR is not set (path to <pkg>/runtimes/<rid>/native)")
    endif()
    set_target_properties(${TARGET_NAME} PROPERTIES PREFIX "lib")
    set_target_properties(${TARGET_NAME} PROPERTIES
        BUILD_RPATH "$ORIGIN"
        INSTALL_RPATH "$ORIGIN")
    foreach(lib avformat avcodec avutil)
        file(GLOB FFLIB_${lib} "${FFMPEG_RUNTIME_DIR}/lib${lib}.so*")
        if(NOT FFLIB_${lib})
            message(FATAL_ERROR "lib${lib}.so* not found in FFMPEG_RUNTIME_DIR=${FFMPEG_RUNTIME_DIR}")
        endif()
        list(GET FFLIB_${lib} 0 FFLIB_${lib}_FIRST)
        target_link_libraries(${TARGET_NAME} PRIVATE "${FFLIB_${lib}_FIRST}")
    endforeach()
endif()
