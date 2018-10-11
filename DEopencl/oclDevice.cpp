#include <oclDevice.h>

vector<shared_ptr<oclDevice>> oclDevice::queryDevices(
    shared_ptr<oclPlatform> platform, cl_device_type device_types) 
{
    vector<shared_ptr<oclDevice>> devices;

    cl_device_id device_ids[8];
	cl_uint numDevices;

	if (clGetDeviceIDs(platform->getId(), device_types, 8, device_ids, &numDevices) == CL_SUCCESS) {
		for (int n = 0; n < numDevices; n++) {
            devices.push_back(make_shared<oclDevice>(device_ids[n]));
        }
	}

    return devices;
}

oclDevice::oclDevice(cl_device_id id) : id(id) {

}

cl_device_id oclDevice::getId() {
    return id;
}