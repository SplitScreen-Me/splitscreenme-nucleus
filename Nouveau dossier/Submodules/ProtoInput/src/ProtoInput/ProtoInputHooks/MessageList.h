#pragma once
#include <vector>
#include <string>

namespace Proto
{

struct MessageListItem
{
	unsigned int messageID;
	std::string name;
	bool blocked = false;

	MessageListItem(unsigned int _messageID, const char* _name) : messageID(_messageID), name(_name), blocked(false){}
};

class MessageList
{
	static std::vector<unsigned int> indexTable;
	
	static void GenerateIndexTable();

public:
	static std::vector<MessageListItem> messages;

	static void ShowUI();
	
	static bool IsBlocked(unsigned int message);
	static bool* GetBlocked(unsigned int message);
};

}
