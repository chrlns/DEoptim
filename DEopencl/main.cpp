#pragma comment(lib, "opencl.lib")

#include <iostream>
#include <random>
#include <climits>

#ifndef CL_TARGET_OPENCL_VERSION
#define CL_TARGET_OPENCL_VERSION 120
#endif
#include <CL/cl.h>

#include <oclDevice.h>
#include <oclPlatform.h>
#include <oclUtils.h>

using namespace std;

void createRandomBuffer(cl_uint buf[], cl_uint len) {
	std::random_device rd;
	std::mt19937 rng(rd());
	std::uniform_int_distribution<unsigned int> dist(0, UINT_MAX);

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

	cl_uint NP = 1024; // Number of individuals // TODO cli param
	cl_uint N = 3; // 3 floats per individual
	cl_uint Gmax = 100; // Number of generations

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

	const char* options = "-cl-std=CL1.2 -O3";
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
	printCLStatus(err, "clCreateKernel population_init");
	cl_kernel kern_population_mutate = clCreateKernel(program, "population_mutate", &err);
	printCLStatus(err, "clCreateKernel population_mutate");
	cl_kernel kern_population_cross = clCreateKernel(program, "population_cross", &err);
	printCLStatus(err, "clCreateKernel population_cross");
	cl_kernel kern_population_select = clCreateKernel(program, "population_select", &err);
	printCLStatus(err, "clCreateKernel population_select");

	// Create population buffer
	cl_mem buf_population_0 = clCreateBuffer(context, CL_MEM_READ_WRITE, sizeof(cl_float) * NP * N, nullptr, &err);
	printCLStatus(err, "clCreateBuffer population_0");
	cl_mem buf_population_1 = clCreateBuffer(context, CL_MEM_READ_WRITE, sizeof(cl_float) * NP * N, nullptr, &err);
	printCLStatus(err, "clCreateBuffer population_1");

	// Create buffers for attribute limits
	cl_float* attr_min_limit = new cl_float[N];
	cl_float* attr_max_limit = new cl_float[N];
	for (int n = 0; n < N; n++) {
		attr_max_limit[n] = 5.12;
		attr_min_limit[n] = -5.12f;
	}
	cl_mem buf_attr_min_limit = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_float) * N, (void*)attr_min_limit, &err);
	printCLStatus(err, "clCreateBuffer attr_min_limit");
	cl_mem buf_attr_max_limit = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_float) * N, (void*)attr_max_limit, &err);
	printCLStatus(err, "clCreateBuffer attr_max_limit");

	// Create buffers for seed
	cl_uint* seed = new cl_uint[2 * NP];
	createRandomBuffer(seed, 2 * NP);
	cl_mem buf_seed = clCreateBuffer(context, CL_MEM_USE_HOST_PTR, sizeof(cl_uint) * NP * 2, (void*)seed, &err);
	printCLStatus(err, "clCreateBuffer seed");

	cl_mem buf_costs = clCreateBuffer(context, CL_MEM_WRITE_ONLY, sizeof(cl_float) * NP, NULL, &err);
	printCLStatus(err, "clCreateBuffer costs");

	// Set the kernel arguments
	err = clSetKernelArg(kern_population_init, 0, sizeof(cl_mem), &buf_population_0);
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

	// Default arguments of mutation kernel
	cl_float F = 0.8f; // TODO paramterize
	err = clSetKernelArg(kern_population_mutate, 0, sizeof(cl_uint), &NP);
	printCLStatus(err, "clSetKernelArg population_mutate 0");
	err = clSetKernelArg(kern_population_mutate, 1, sizeof(cl_float), &F);
	printCLStatus(err, "clSetKernelArg population_mutate 1");
	err = clSetKernelArg(kern_population_mutate, 4, sizeof(cl_uint), &N);
	printCLStatus(err, "clSetKernelArg population_mutate 4");
	err = clSetKernelArg(kern_population_mutate, 5, sizeof(cl_mem), &buf_seed);
	printCLStatus(err, "clSetKernelArg population_mutate 5");

	// Default arguments of crossover kernel
	cl_float CR = 0.6f;
	err = clSetKernelArg(kern_population_cross, 0, sizeof(cl_float), &CR);
	printCLStatus(err, "clSetKernelArg population_cross 0");
	err = clSetKernelArg(kern_population_cross, 3, sizeof(cl_uint), &N);
	printCLStatus(err, "clSetKernelArg population_cross 3");
	err = clSetKernelArg(kern_population_cross, 4, sizeof(cl_mem), &buf_seed);
	printCLStatus(err, "clSetKernelArg population_cross 4");

	// Default arguments of selection kernel
	err = clSetKernelArg(kern_population_select, 2, sizeof(cl_uint), &N);
	printCLStatus(err, "clSetKernelArg population_select 2");
	err = clSetKernelArg(kern_population_select, 3, sizeof(cl_mem), &buf_costs);
	printCLStatus(err, "clSetKernelArg population_select 3");

	cl_float* costs = new cl_float[NP];

	// DE Optimization Loop
	for (cl_uint G = 1; G <= Gmax; G++) {
		cout << "Gen. #" << G << " min = ";

		cl_mem pop_old, pop_new;

		if (G % 2 == 1) {
			pop_old = buf_population_0;
			pop_new = buf_population_1;
		} else {
			pop_old = buf_population_1;
			pop_new = buf_population_0;
		}

		// Mutation
		err = clSetKernelArg(kern_population_mutate, 2, sizeof(cl_mem), &pop_old);
		printCLStatus(err, "clSetKernelArg population_mutate 2");
		err = clSetKernelArg(kern_population_mutate, 3, sizeof(cl_mem), &pop_new);
		printCLStatus(err, "clSetKernelArg population_mutate 3");

		err = clEnqueueNDRangeKernel(queue, kern_population_mutate, workDim, NULL, &globalWorkSize, &localWorkSize, 0, NULL, NULL);
		printCLStatus(err, "clEnqueueNDRangeKernel population_mutate");

		// Crossover
		err = clSetKernelArg(kern_population_cross, 1, sizeof(cl_mem), &pop_old);
		printCLStatus(err, "clSetKernelArg population_cross 1");
		err = clSetKernelArg(kern_population_cross, 2, sizeof(cl_mem), &pop_new);
		printCLStatus(err, "clSetKernelArg population_cross 2");

		err = clEnqueueNDRangeKernel(queue, kern_population_cross, workDim, NULL, &globalWorkSize, &localWorkSize, 0, NULL, NULL);
		printCLStatus(err, "clEnqueueNDRangeKernel population_cross");

		// Selection
		err = clSetKernelArg(kern_population_select, 0, sizeof(cl_mem), &pop_old);
		printCLStatus(err, "clSetKernelArg population_select 0");
		err = clSetKernelArg(kern_population_select, 1, sizeof(cl_mem), &pop_new);
		printCLStatus(err, "clSetKernelArg population_select 1");

		err = clEnqueueNDRangeKernel(queue, kern_population_select, workDim, NULL, &globalWorkSize, &localWorkSize, 0, NULL, NULL);
		printCLStatus(err, "clEnqueueNDRangeKernel population_select");

		// Read results
		err = clEnqueueReadBuffer(queue, buf_costs, true, 0, sizeof(cl_float) * NP, costs, 0, nullptr, nullptr);
		printCLStatus(err, "clEnqueueReadBuffer");

		cl_uint min = 0;
		for (cl_uint n = 1; n < NP; n++) {
			if (costs[n] < costs[min])
				min = n;
		}
		cout << costs[min] << endl;
		if (costs[min] <= 1e-6) {
			cout << "VTR" << endl;
			break;
		}
	}

	cout << "Kernel running...";
	err = clFinish(queue);
	printCLStatus(err, "clFinish");
	cout << "OK" << endl;

	return 0;
}
