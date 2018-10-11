#include "cl_utils.h"

#include <fstream>
#include <iostream>


using namespace std;

/**
 * Loads a text file containing OpenCL kernels and compiles the program
 * using clCreateProgramWithSource.
 */
cl_program clCreateProgramFromFile(cl_context context, const char* file, int maxLineLength, int maxFileLength, cl_int* err)
{
	cl_uint n;
	ifstream in(file);

	if (in.is_open()) {
		char **strings = new char*[maxFileLength];

		for (n = 0; n < maxFileLength && !in.eof(); n++) {
			strings[n] = new char[maxLineLength];
			in.getline(strings[n], maxLineLength);
		}

		cl_program program = clCreateProgramWithSource(context, n, (const char**)strings, NULL, err);

		// Free resources
		for (int i = 0; i < n; i++) {
			delete[] strings[i];
		}
		delete[] strings;
		
		return program;
	}

	cerr << "Could not open " << file << " for reading" << endl;
	*err = -1;
	return 0;
}

cl_program clCreateProgramFromFile(cl_context context, const char* file, cl_int* err)
{
	return clCreateProgramFromFile(context, file, MAX_LINE_LENGTH, MAX_FILE_LENGTH, err);
}

void printCLStatus(cl_int errNum, string msg)
{
	if (errNum == CL_SUCCESS)
		return;

	cout << "OpenCL result = ";
	switch (errNum) {
	case CL_SUCCESS:
		cout << "CL_SUCCESS";
		return;
	case CL_OUT_OF_HOST_MEMORY:
		cout << "CL_OUT_OF_HOST_MEMORY";
		break;
	case CL_INVALID_WORK_GROUP_SIZE:
		cout << "CL_INVALID_WORK_GROUP_SIZE";
		break;
	case CL_INVALID_WORK_ITEM_SIZE:
		cout << "CL_INVALID_WORK_ITEM_SIZE";
		break;
	default:
		cout << errNum;
	}
	cout << endl;
	exit(errNum);
}