#pragma once

#include <memory>
#include <vector>

#define CL_TARGET_OPENCL_VERSION 120
#include <CL/cl.h>

using namespace std;

class oclPlatform {
    private:
        cl_platform_id id;

    public:
        static vector<shared_ptr<oclPlatform>> queryPlatforms();
        static shared_ptr<oclPlatform> queryDefaultPlatform();

        oclPlatform(cl_platform_id id);

        
        void printDeviceInfo(cl_device_id);
        void printPlatformInfo();
};