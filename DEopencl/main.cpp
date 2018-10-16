#pragma comment(lib, "opencl.lib")

#include <iostream>
#include <random>

#ifndef CL_TARGET_OPENCL_VERSION
#define CL_TARGET_OPENCL_VERSION 120
#endif
#include <CL/cl.h>

#include <oclDevice.h>
#include <oclPlatform.h>
#include <oclUtils.h>

using namespace std;

void createRandomBuffer(float min, float max, float buf[], int len) {
	std::random_device rd;
	std::mt19937 rng(rd());
	std::uniform_real_distribution<float> dist(min, max);

	for (int n = 0; n < len; n++) {
		buf[n] = dist(rng);
	}
}

void printBuildError(cl_program program, cl_device_id device) {
	// Determine the size of the log
	size_t log_size;
	clGetProgramBuildInfo(program, device, CL_PROGRAM_BUILD_LOG, 0, NULL, &log_size);

	// Allocate memory for the log
	char *log = new char[log_size];

	// Get the log
	clGetProgramBuildInfo(program, device, CL_PROGRAM_BUILD_LOG, log_size, log, NULL);

	// Print the log
	cerr << log << endl;
	delete[] log;
}

int main(int argc, char* argv[]) {
	shared_ptr<oclPlatform> defaultPlatform = oclPlatform::queryDefaultPlatform();

	if (defaultPlatform == nullptr) {
		cerr << "No default OpenCL platform found! Exit." << endl;
		return 1;
	}

	defaultPlatform->printPlatformInfo();

	cl_int errNum;
	cl_context context = defaultPlatform->createDefaultGPUContext(&errNum);
	if (errNum != CL_SUCCESS) {
		switch (errNum) {
		case CL_INVALID_PLATFORM:
			cerr << "clCreateContextFromType CL_INVALID_PLATFORM" << endl;
			break;
		case CL_INVALID_PROPERTY:
			cerr << "clCreateContextFromType CL_INVALID_PROPERTY" << endl;
			break;
		default:
			cerr << "clCreateContextFromType error " << errNum << endl;
		}

	}
	cout << "Context for default GPU device created." << endl;

	vector<shared_ptr<oclDevice>> devices = oclDevice::queryDevices(
		defaultPlatform, CL_DEVICE_TYPE_GPU | CL_DEVICE_TYPE_DEFAULT);
	if (devices.empty()) {
		cerr << "Could not find any suitable GPU devices!" << endl;
		return 3;
	}
	shared_ptr<oclDevice> device = devices[0];

	int NP = 8096; // Number of individuals // TODO cli param
	int N = 4; // 4 floats per individual


	// Create command queue
	cl_command_queue queue = clCreateCommandQueue(context, device->getId(), 0, &errNum);
	if (errNum != CL_SUCCESS) {
		cerr << "Error clCreateCommandQueue()" << endl;
	}

	// Load and Build Program and Kernels
	cl_program program = clCreateProgramFromFile(context, "DE.cl", &errNum); // In-Ordern kernel
	if (errNum != CL_SUCCESS) {
		switch (errNum) {
		case CL_INVALID_CONTEXT:
			cerr << "clCreateProgramFromFile CL_INVALID_CONTEXT" << endl;
			break;
		default:
			cerr << "clCreateProgramFromFile error " << errNum << endl;
		}
	}

	const char* options = "-cl-std=CL1.2";
	cl_device_id device_list[1] = {device->getId()};
	errNum = clBuildProgram(program, 1, device_list, options, nullptr, nullptr);
	if (errNum != CL_SUCCESS) {
		switch (errNum) {
		case CL_INVALID_PROGRAM:
			cerr << "clBuildProgramm() CL_INVALID_PROGRAM" << endl;
			break;
		case CL_BUILD_PROGRAM_FAILURE:
			cerr << "clBuildProgramm() CL_BUILD_PROGRAM_FAILURE" << endl;
			printBuildError(program, device->getId());
			break;
		default:
			cerr << "Error clBuildProgramm()" << endl;
		}
		return -1;
	}
	cout << "Successfully built OpenCL program." << endl;

	cl_int err;
	cl_kernel kern_population_init = clCreateKernel(program, "population_init", &err);
	if (err != CL_SUCCESS) {
		switch (err) {
		default:
			cerr << "Error clCreateKernel() = " << err << endl;
		}
	}

	return 0;
}
