cmake_minimum_required(VERSION 2.8)
find_package(Threads)
add_definitions("-std=c++14")
add_definitions("-DFMT_HEADER_ONLY")
add_definitions("-DASIO_STANDALONE")
include_directories("../../Lib/utf8")
include_directories("../../Lib/cppformat")
include_directories("../../Lib/asio/include")
include_directories(".")

file(GLOB_RECURSE SRCS RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cpp")
string(REGEX REPLACE "CMakeFiles/[^;]+;?" "" SRCS "${SRCS}")
add_executable(Server ${SRCS})
target_link_libraries(Server dl ${CMAKE_THREAD_LIBS_INIT})
