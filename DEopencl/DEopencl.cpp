#pragma comment(lib, "opencl.lib")

#include <iostream>
#include <random>

#define CL_TARGET_OPENCL_VERSION 120
#include <CL/cl.h>

#include <oclPlatform.h>

#ifndef WIN32
#define __stdcall
#endif

using namespace std;

void printDeviceInfo(cl_device_id id) {
	char* buf = new char[512];
	cl_uint buf_uint;
	cl_ulong buf_ulong;
	size_t bufSize;

	clGetDeviceInfo(id, CL_DEVICE_NAME, 512, buf, &bufSize);
	cout << "\tDevice " << id << " " << (char*)buf << endl;

	clGetDeviceInfo(id, CL_DEVICE_MAX_COMPUTE_UNITS, sizeof(cl_uint), &buf_uint, &bufSize);
	cout << "\t\tMax. compute units = " << buf_uint << endl;

	clGetDeviceInfo(id, CL_DEVICE_GLOBAL_MEM_SIZE, sizeof(cl_ulong), &buf_ulong, &bufSize);
	cout << "\t\tMax. global mem size = " << buf_ulong / 1024 / 1024 << " MiB" << endl;
}

void printPlatformInfo(cl_platform_id id) {
#define BUF_LIMIT 512
	char* buf = new char[BUF_LIMIT];
	size_t bufSize;

	clGetPlatformInfo(id, CL_PLATFORM_VERSION, BUF_LIMIT, buf, &bufSize);
	cout << id << ": " << (char*)buf;

	clGetPlatformInfo(id, CL_PLATFORM_NAME, BUF_LIMIT, buf, &bufSize);
	cout << " " << (char*)buf << endl;

	clGetPlatformInfo(id, CL_PLATFORM_VENDOR, BUF_LIMIT, buf, &bufSize);
	cout << "Vendor: " << (char*)buf << endl;

	clGetPlatformInfo(id, CL_PLATFORM_EXTENSIONS, BUF_LIMIT, buf, &bufSize);
	cout << "Extensions: " << (char*)buf << endl;

	// Device information
	cl_device_id devices[8];
	cl_uint numDevices;
	if (clGetDeviceIDs(id, CL_DEVICE_TYPE_ALL, 8, devices, &numDevices) != CL_SUCCESS) {
		cerr << "Error clGetDeviceIDs" << endl;
	} else {
		for (cl_uint n = 0; n < numDevices; n++) {
			printDeviceInfo(devices[n]);
		}
	}

	cout << endl;
}

void __stdcall pfn_notify(const char* errinfo,
	const void* private_info,
	size_t cb, void* user_data)
{
	fprintf(stderr, "OpenCL Error (via pfn_notify): %s\n", errinfo);
	flush(cout);
}

cl_context createContext(cl_platform_id platform, cl_int* errNum) {
	cl_context_properties properties[] = {
		CL_CONTEXT_PLATFORM,
		(cl_context_properties)platform,
		0
	};

	cl_context context = clCreateContextFromType(properties, CL_DEVICE_TYPE_GPU | CL_DEVICE_TYPE_DEFAULT,
		&pfn_notify,
		NULL,
		errNum);

	return context;
}

void createRandomBuffer(float min, float max, float buf[], int len) {
	std::random_device rd;
	std::mt19937 rng(rd());
	std::uniform_real_distribution<float> dist(min, max);

	for (int n = 0; n < len; n++) {
		buf[n] = dist(rng);
	}
}

int main(int argc, char* argv[]) {
	shared_ptr<oclPlatform> defaultPlatform = oclPlatform::queryDefaultPlatform();

	if (defaultPlatform == nullptr) {
		cerr << "No default OpenCL platform found! Exit." << endl;
		return 1;
	}

	defaultPlatform->printPlatformInfo();

	cl_int errNum;
	cl_context context = createContext(platforms[0], &errNum);
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

	int N = 8096;

	// We need buffers, more buffers...
	// a read-only 1D-buffer containing the sample data float2[NUM_SAMPLES] buf_samples_1d
	// a read-write 2D-buffer containing temporary evaluation data float[NUM_INDIVIDUALS x NUM_SAMPLES] buf_sample_error_2d
	// a read-write 1D-buffer containing the individuals float4[NUM_INDIVIDUALS] buf_pop_1d
	// a read-write 1D-buffer containing the individuals' errors float[NUM_INDIVIDUALS] buf_pop_error_1d

	// We need kernels:
	// pop_init.cl (buf_rw_population): creates randomized individuals
	// pop_eval.cl (buf_ro_samples, buf_rw_population, buf_rw_population_error, int pop_idx)

	float* buf = new float[1024 * 1024];
	createRandomBuffer(-10.0f, 10.0f, buf, 1024 * 1024);

	return 0;
}
