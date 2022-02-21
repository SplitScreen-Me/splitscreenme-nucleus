#include "TrackedInstances.h"

std::unordered_map<ProtoInstanceHandle, Proto::ProtoInstance> Proto::instances;
ProtoInstanceHandle Proto::instanceCounter = 1;
