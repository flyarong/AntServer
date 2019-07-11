using UnityEngine;

namespace Skynet.DotNetClient.Login.TCP
{
    using System;
    using System.Net.Sockets;

    using System.Net;
    using System.Threading;
    
    //https://github.com/cloudwu/skynet/wiki/LoginServer
    
    public class LoginClient :IDisposable
    {
        public event Action<NetWorkState> NetWorkStateChangedEvent;
        
        private ManualResetEvent _timeoutEvent;
        private int _timeoutMSec = 8000;    //connect timeout count in millisecond
        
        private NetWorkState _netWorkState;   //current network state
        private Socket _socket;
        private bool _disposed;

        private Protocol _protocol;
        private EventManager _eventManager;

        public LoginClient()
        {
            _disposed = false;
            _netWorkState = NetWorkState.CLOSED;
            _timeoutMSec = 8000;
            _timeoutEvent = new ManualResetEvent(false);
        }

        public void Connect(string host, int port, AuthPackageReq req, Action<int, AuthPackageResp> loginCallBack)
        {
            if (_netWorkState != NetWorkState.CLOSED)
            {
                Debug.Log("LoginClient has connect action");
                return;
            }
            
            _timeoutEvent.Reset();
            _eventManager = new EventManager(req, loginCallBack, this);
            
            NetWorkChanged(NetWorkState.CONNECTING);
			
            IPAddress ipAddress = null;

            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(host).AddressList;
                foreach (var item in addresses)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = item;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                NetWorkChanged(NetWorkState.ERROR);
                return;
            }

            if (ipAddress == null)
            {
                throw new Exception("can not parse host : " + host);
            }

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(ipAddress, port);

            _socket.BeginConnect(ie, new AsyncCallback((result) =>
            {
                try
                {
                    _socket.EndConnect(result);
                    _protocol = new Protocol(this, this._socket);
                    NetWorkChanged(NetWorkState.CONNECTED);
                }
                catch (SocketException e)
                {
                    if (_netWorkState != NetWorkState.TIMEOUT)
                    {
                        NetWorkChanged(NetWorkState.ERROR);
                    }
                    Dispose();
                }
                finally
                {
                    _timeoutEvent.Set();
                }
            }), _socket);

            if (_timeoutEvent.WaitOne(_timeoutMSec, false))
            {
                if (_netWorkState != NetWorkState.CONNECTED && _netWorkState != NetWorkState.ERROR)
                {
                    NetWorkChanged(NetWorkState.TIMEOUT);
                    Dispose();
                }
            }
        }

        public void Request(byte[] packet)
        {
            _protocol.Send(packet);
        }

        internal void ProcessMessage(byte[] bytes)
        {
            _eventManager.InvokeCallBack(bytes);
        }
        
        public void Disconnect()
        {
            Dispose();
            NetWorkChanged(NetWorkState.DISCONNECTED);
        }

        private void NetWorkChanged(NetWorkState state)
        {
            _netWorkState = state;
            if (NetWorkStateChangedEvent != null)
            {
                NetWorkStateChangedEvent(state);
            }
        }
        
        public void Dispose() 
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        // The bulk of the clean-up code
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
                catch (Exception)
                {
                    //todo : 有待确定这里是否会出现异常，这里是参考之前官方github上pull request。emptyMsg
                }

                _disposed = true;
            }
        }
    }
}