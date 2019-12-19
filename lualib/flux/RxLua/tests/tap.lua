describe('tap', function()
  it('does not create a subscription', function()
    local observable = Rx.Observable.create(error)
    expect(observable.tap).to_not.fail()
  end)

  it('runs the specified onNext function', function()
    local onNext = spy()
    local observable = Rx.Observable.create(function(observer)
      observer:onNext(1)
      observer:onCompleted()
    end):tap(onNext):subscribe()
    expect(onNext).to.equal({{1}})
  end)

  it('calls onError if the onNext callback errors', function()
    expect(Rx.Observable.of(1):tap(error)).to.produce.error()
  end)

  it('runs the specified onError function', function()
    local onError = spy()
    local observable = Rx.Observable.create(function(observer)
      observer:onError('message')
    end):tap(_, onError):subscribe(_, function() end)
    expect(onError).to.equal({{'message'}})
  end)

  it('calls onError if the onError callback errors', function()
    expect(Rx.Observable.throw():tap(nil, error)).to.produce.error()
  end)

  it('runs the specified onCompleted function', function()
    local onCompleted = spy()
    local observable = Rx.Observable.create(function(observer)
      observer:onCompleted()
    end):tap(_, _, onCompleted):subscribe()
    expect(#onCompleted).to.equal(1)
  end)

  it('calls onError if the onCompleted callback errors', function()
    expect(Rx.Observable.of(1):tap(nil, nil, error)).to.produce.error()
  end)
end)