cmake_minimum_required (VERSION 3.12)
project (decl)

find_package (OpenCL REQUIRED)
if (OpenCL_FOUND)
    link_libraries(${OpenCL_LIBRARIES})
endif()

include_directories(".")

add_executable(decl DEopencl.cpp oclPlatform.cpp)