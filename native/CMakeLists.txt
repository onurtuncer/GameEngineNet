cmake_minimum_required(VERSION 3.22)
project(HazelBridgeRoot LANGUAGES CXX)

# -------- Options --------
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Nice MSVC defaults
if(MSVC)
  # MD/MDd CRT
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>DLL")
  add_compile_options(/permissive- /Zc:preprocessor /Zc:__cplusplus /W4)
else()
  add_compile_options(-Wall -Wextra -Wpedantic)
endif()

# -------- Paths --------
# Adjust if your Hazel submodule lives elsewhere
set(HAZEL_DIR "${CMAKE_CURRENT_LIST_DIR}/../vendor/Hazel")

# -------- Hazel engine as subdir --------
# Hazel itself usually brings spdlog; we will consume Hazel’s targets.
# If Hazel has its own top-level CMake, this should work:
if(EXISTS "${HAZEL_DIR}/CMakeLists.txt")
  add_subdirectory("${HAZEL_DIR}" "${CMAKE_BINARY_DIR}/_hazel_build" EXCLUDE_FROM_ALL)
else()
  message(FATAL_ERROR "Hazel submodule not found at: ${HAZEL_DIR}")
endif()

# -------- Bridge target --------
# Sources for your bridge/shim (adjust file list to match your repo)
set(HAZEL_BRIDGE_SOURCES
  HazelBridge/hazel_bridge.h
  HazelBridge/hazel_bridge.cpp
  HazelBridge/logging_bridge.h
  HazelBridge/logging_bridge.cpp
  HazelBridge/hazel_entry_bridge.cpp   # entry replacement (hz_init/hz_run/hz_shutdown)
)

add_library(HazelBridge SHARED ${HAZEL_BRIDGE_SOURCES})

# We don’t want Hazel’s default main(); guard it out when compiling bridge
target_compile_definitions(HazelBridge PRIVATE HAZEL_NO_MAIN SPDLOG_FMT_EXTERNAL=0)

# If Hazel exports a target called "Hazel" or "Hazel::Hazel", link it.
# Many forks name it "Hazel". If your fork uses a namespaced target, prefer that.
# Try both gracefully:
set(_hazel_target "")
if(TARGET Hazel::Hazel)
  set(_hazel_target Hazel::Hazel)
elseif(TARGET Hazel)
  set(_hazel_target Hazel)
else()
  message(WARNING "Could not find a Hazel target (Hazel::Hazel or Hazel). Ensure Hazel CMake defines it.")
endif()

if(_hazel_target)
  target_link_libraries(HazelBridge PRIVATE ${_hazel_target})
endif()

# If Hazel doesn’t re-export spdlog, include headers directly as a fallback
# (harmless when Hazel already re-exports)
target_include_directories(HazelBridge PRIVATE
  "${HAZEL_DIR}/vendor/spdlog/include"
  "${HAZEL_DIR}"                                 # for <Hazel.h> if you include umbrella
)

# Windows export name
set_target_properties(HazelBridge PROPERTIES OUTPUT_NAME "HazelBridge")

# -------- Output dirs (nice & predictable) --------
set(_out "${CMAKE_BINARY_DIR}/out")
foreach(cfg IN ITEMS Debug Release RelWithDebInfo MinSizeRel)
  string(TOUPPER "${cfg}" CFGU)
  set_target_properties(HazelBridge PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY_${CFGU} "${_out}/${cfg}/lib"
    LIBRARY_OUTPUT_DIRECTORY_${CFGU} "${_out}/${cfg}/bin"
    RUNTIME_OUTPUT_DIRECTORY_${CFGU} "${_out}/${cfg}/bin"
  )
endforeach()

# -------- Helpful message --------
message(STATUS "Build HazelBridge with e.g.:")
message(STATUS "  cmake -S . -B build -DCMAKE_BUILD_TYPE=Release")
message(STATUS "  cmake --build build --config Release")
message(STATUS "Artifacts in: ${CMAKE_BINARY_DIR}/out/<Config>/bin/HazelBridge.dll")
