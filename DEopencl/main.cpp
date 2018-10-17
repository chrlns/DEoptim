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

void createRandomBuffer(cl_float min, cl_float max, cl_float buf[], cl_uint len) {
	std::random_device rd;
	std::mt19937 rng(rd());
	std::uniform_real_distribution<float> dist(min, max);

	for (cl_uint n = 0; n < len; n++) {
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

	cl_uint NP = 8096; // Number of individuals // TODO cli param
	cl_uint N = 4; // 4 floats per individual


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

	// Create population buffer
	cl_mem buf_population = clCreateBuffer(context, CL_MEM_READ_WRITE, sizeof(cl_float) * NP * N, nullptr, &err);
	printCLStatus(err, "clCreateBuffer population");

	// Create buffers for attribute limits
	cl_float* attr_min_limit = new cl_float[N];
	cl_float* attr_max_limit = new cl_float[N];
	for (int n = 0; n < N; n++) {
		attr_max_limit[n] = 10.0f;
		attr_min_limit[n] = -10.0f;
	}
	cl_mem buf_attr_min_limit = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_float) * N, (void*)attr_min_limit, &err);
	printCLStatus(err, "clCreateBuffer attr_min_limit");
	cl_mem buf_attr_max_limit = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_float) * N, (void*)attr_max_limit, &err);
	printCLStatus(err, "clCreateBuffer attr_max_limit");

	// Create buffers for seed
	cl_float* seed = new cl_float[2 * NP];
	createRandomBuffer(-10, 10, seed, 2 * NP);
	cl_mem buf_seed = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_float) * N * 2, (void*)seed, &err);
	printCLStatus(err, "clCreateBuffer seed");

	// Set the kernel arguments
	err = clSetKernelArg(kern_population_init, 0, sizeof(cl_mem), &buf_population);
	printCLStatus(err, "clSetKernelArg population_init 0");
	err = clSetKernelArg(kern_population_init, 1, sizeof(cl_uint), &N);
	printCLStatus(err, "clSetKernelArg population_init 1");
	err = clSetKernelArg(kern_population_init, 2, sizeof(cl_mem), &buf_attr_min_limit);
	printCLStatus(err, "clSetKernelArg population_init 2");
	err = clSetKernelArg(kern_population_init, 3, sizeof(cl_mem), &buf_attr_max_limit);
	printCLStatus(err, "clSetKernelArg population_init 3");
	err = clSetKernelArg(kern_population_init, 4, sizeof(cl_mem), &buf_seed);
	printCLStatus(err, "clSetKernelArg population_init 4");

	cl_event wait;
	cl_uint workDim = 1;
	size_t globalWorkSize = NP;
	size_t localWorkSize = 256; // FIXME Determine

	err = clEnqueueNDRangeKernel(queue, kern_population_init, workDim, NULL, &globalWorkSize, &localWorkSize, 0, NULL, NULL);
	printCLStatus(err, "clEnqueueNDRangeKernel population_init");

	cout << "Kernel running...";
	err = clFinish(queue);
	printCLStatus(err, "clFinish");
	cout << "OK" << endl;

	cl_float* population = new cl_float[NP * N];
	err = clEnqueueReadBuffer(queue, buf_population, true, 0, sizeof(cl_float) * NP * N, population, 0, nullptr, nullptr);
	printCLStatus(err, "clEnqueueReadBuffer");

	for (int n = 0; n < 15; n++) {
		cout << population[n] << " ";
	}
	cout << endl;

	return 0;
}
