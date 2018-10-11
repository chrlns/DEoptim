#pragma once

#include <memory>
#include <vector>

#define CL_TARGET_OPENCL_VERSION 120
#include <CL/cl.h>

using namespace std;

class oclPlatform {
    private:
        cl_context current_context;
        cl_platform_id id;

    public:
        static vector<shared_ptr<oclPlatform>> queryPlatforms();
        static shared_ptr<oclPlatform> queryDefaultPlatform();

        oclPlatform(cl_platform_id id);

        cl_context createContext(cl_device_type type, cl_int* errNum);
        cl_context createDefaultGPUContext(cl_int* errNum);
        void printDeviceInfo(cl_device_id);
        void printPlatformInfo();
};