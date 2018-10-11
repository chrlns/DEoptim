#pragma once

#define CL_TARGET_OPENCL_VERSION 120
#include <CL/cl.h>

#include <string>

using namespace std;

#define MAX_LINE_LENGTH 256
#define MAX_FILE_LENGTH 2048

cl_program clCreateProgramFromFile(cl_context context, const char* file, cl_int* err);
cl_program clCreateProgramFromFile(cl_context context, const char* file, int maxLineLength, int maxFileLength, cl_int* err);

void printCLStatus(cl_int errNum, string msg);