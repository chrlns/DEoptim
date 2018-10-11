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
	//cl_context context = createContext(platforms[0], &errNum);
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
