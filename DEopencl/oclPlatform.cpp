#include <oclPlatform.h>

#include <iostream>

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


oclPlatform::oclPlatform(int id) {

}