#pragma once

#include <memory>
#include <vector>

#ifndef CL_TARGET_OPENCL_VERSION
#define CL_TARGET_OPENCL_VERSION 120
#endif
#include <CL/cl.h>

#include <oclPlatform.h>

using namespace std;

class oclDevice {
    private:
        cl_device_id id;

    public:
        static vector<shared_ptr<oclDevice>> queryDevices(
            shared_ptr<oclPlatform>, cl_device_type);

        oclDevice(cl_device_id);
        cl_device_id getId();
};