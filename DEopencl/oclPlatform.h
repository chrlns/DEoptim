#pragma once

#include <memory>
#include <vector>

#include <CL/cl.h>

using namespace std;

class oclPlatform {
    public:
        static vector<shared_ptr<oclPlatform>> queryPlatforms();
        static shared_ptr<oclPlatform> queryDefaultPlatform();

        oclPlatform(int id);
};