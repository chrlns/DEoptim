#include <oclPlatform.h>

#include <iostream>

#ifndef WIN32
#define __stdcall
#endif

void __stdcall pfn_notify(const char* errinfo,
	const void* private_info,
	size_t cb, void* user_data)
{
	fprintf(stderr, "OpenCL Error (via pfn_notify): %s\n", errinfo);
	flush(cout);
}

vector<shared_ptr<oclPlatform>> oclPlatform::queryPlatforms() {
	cl_platform_id platform_ids[8];
	cl_uint numPlatforms;

	if (clGetPlatformIDs(8, platform_ids, &numPlatforms) != CL_SUCCESS) {
		cerr << "Error clGetPlatformIDs" << endl;
	}

    vector<shared_ptr<oclPlatform>> platforms;

    for (auto n = 0; n < numPlatforms; n++) {
        platforms.push_back(make_shared<oclPlatform>(platform_ids[n]));
    }

    return platforms;
}

shared_ptr<oclPlatform> oclPlatform::queryDefaultPlatform() {
    cl_platform_id platform_id[1];
	cl_uint numPlatforms;

	if (clGetPlatformIDs(1, platform_id, &numPlatforms) != CL_SUCCESS) {
		cerr << "Error clGetPlatformIDs" << endl;
        return make_shared<oclPlatform>(nullptr);
	}

    if (numPlatforms == 0) {
        return make_shared<oclPlatform>(nullptr);
    }

    return make_shared<oclPlatform>(platform_id[0]);
}


oclPlatform::oclPlatform(cl_platform_id id) : id(id) {

}

cl_context oclPlatform::createContext(cl_device_type type, cl_int* errNum) {
	cl_context_properties properties[] = {
		CL_CONTEXT_PLATFORM,
		(cl_context_properties)id,
		0
	};

	cl_context context = clCreateContextFromType(properties, 
        type,
		&pfn_notify,
		NULL,
		errNum);

    this->current_context = context;

	return context;
}

cl_context oclPlatform::createDefaultGPUContext(cl_int* errNum) {
    return createContext(CL_DEVICE_TYPE_GPU | CL_DEVICE_TYPE_DEFAULT, errNum);
}

cl_platform_id oclPlatform::getId() {
    return id;
}

void oclPlatform::printDeviceInfo(cl_device_id id) {
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

void oclPlatform::printPlatformInfo() {
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

