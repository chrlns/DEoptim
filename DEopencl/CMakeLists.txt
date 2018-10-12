cmake_minimum_required (VERSION 3.7)
project (decl)

find_package (OpenCL REQUIRED)
if (OpenCL_FOUND)
    link_libraries(${OpenCL_LIBRARIES})
endif()

include_directories(".")

add_executable(decl main.cpp oclPlatform.cpp oclDevice.cpp cl_utils.cpp)
