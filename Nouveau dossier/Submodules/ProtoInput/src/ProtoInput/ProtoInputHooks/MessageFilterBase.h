#pragma once

namespace Proto
{

template<unsigned int FilterId, unsigned int MsgMin, unsigned int MsgMax = MsgMin>
struct MessageFilterBase
{
	constexpr static unsigned int MessageMin() { return MsgMin; }
	constexpr static unsigned int MessageMax() { return MsgMax; }
	constexpr static unsigned int FilterID() { return FilterId; }
};

}
