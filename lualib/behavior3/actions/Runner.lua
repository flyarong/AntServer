local runner = b3.Class("Runner", b3.Action)
b3.Runner = runner

function runner:ctor(params)
	b3.Action.ctor(self,params)

	self.name = "Runner"
end

function runner:tick(tick)
	return b3.RUNNING
end

return runner