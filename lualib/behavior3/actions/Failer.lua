require 'GameCore.3Party.behavior3.core.Action'

local failer = b3.Class("Failer", b3.Action)
b3.Failer = failer

function failer:ctor(params)
	b3.Action.ctor(self,params)
	
	self.name = "Failer"
end

function failer:tick()
	return b3.FAILURE
end