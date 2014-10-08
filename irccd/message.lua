--
-- irccd plugin by HarpyWar (harpywar@gmail.com)
-- Check message.txt modifications and send into a channel with a delay
--

local util = require "irccd.util"
local thread = require "irccd.thread"
		

function onLoad()
	thread.new(startLoop)
end

-- starts in new thread
function startLoop()
	-- require it here because thread functions does not see global vars
	local server = require "irccd.server"
	local system = require "irccd.system"
	local fs = require "irccd.fs"
	local util = require "irccd.util"

	-- configuration
	local serverName = "wenet"
	local channelName = "#nfk"
	local fileCheckInterval = 30 -- in seconds check file modification
	local fileSendInterval = 15*1000 -- 15 minutes (in milliseconds) interval from file modification to send message (youtube video should be ready)
	local filename = "message.txt"
	
	local messages = {}
	local lastModTime = 0
	
	while true do
		system.usleep(fileCheckInterval)
		local s = server.find(serverName)

		local f = io.open(filename, "r")
		
		local stat = fs.stat(filename)
		if stat.mtime then
			-- file modified time
			local filemtime = stat.mtime:format("%H%M%S")
			
			-- if file was modified
			if not (lastModTime == filemtime) then
				-- add new item to table
				table.insert(messages, {sendtime = system.ticks()+fileSendInterval, text = f:read()})
				lastModTime = filemtime
			end
		end
		
		-- iterate table to find expired messages
		for k,v in pairs(messages) do
			if (system.ticks() > v.sendtime) then
				-- send a message
				s:say(channelName, v.text)
				-- remove item
				table.remove(messages, k)
			end
		end
		
		--print(system.ticks() .. " / " .. #messages)
	end
end
