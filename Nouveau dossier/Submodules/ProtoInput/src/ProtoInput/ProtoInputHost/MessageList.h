#pragma once

#include <vector>
#include <string>

namespace ProtoHost
{

struct MessageListItem
{
	unsigned int messageID;
	std::string name;

	MessageListItem(unsigned int _messageID, const char* _name) : messageID(_messageID), name(_name) {}
};

class MessageList
{
	static std::vector<unsigned int> indexTable;

	static void GenerateIndexTable();

public:
	static std::vector<MessageListItem> messages;

	static bool IsBlocked(unsigned message, const std::vector<unsigned int>& blockedMessages);
};

}
