using System;
using System.Net;
using System.Net.Sockets;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    public abstract class ClientConnection
    {
        protected readonly TcpClient _client;
        protected readonly NetworkStream _stream;

        protected ClientConnection(TcpClient client)
        {
            _client = client;
        }

        protected ClientConnection(TcpClient client, NetworkStream stream)
        {
            _client = client;
            _stream = stream;
        }
    }

    public class OpenConnection : ClientConnection
    {
        public OpenConnection(TcpClient client) : base(client, client.GetStream()) { }
        public OpenConnection(TcpClient client, NetworkStream stream) : base(client, stream) { }

        public ClosedConnection Close()
        {
            _stream.Close();
            return new ClosedConnection(_client);
        }

        public byte[] GetData()
        {
            var data = new byte[4096];
            var bytesRead = 0;

            try
            {
                // blocking operation until client receives message
                bytesRead = _stream.Read(data, 0, 4096);
            }
            catch (Exception ex)
            {
                // Swallow it
            }

            return data;
        }
    }

    public class ClosedConnection : ClientConnection
    {
        public ClosedConnection(TcpClient client) : base(client) { }

        public OpenConnection Open() { return new OpenConnection(_client); }
        public DisposedConnection Dispose()
        {
            _stream.Dispose();
            _client.Close();
            return new DisposedConnection(_client);
        }
    }

    public class DisposedConnection : ClientConnection
    {
        public DisposedConnection(TcpClient client) : base(client) { }
    }

    public static class Connections
    {
        public static Func<ClosedConnection, IPair<Unit, OpenConnection>> Open()
        {
            return State.Modify<ClosedConnection, OpenConnection>(c => c.Open());
        }

        public static Func<OpenConnection, IPair<byte[], OpenConnection>> GetData()
        {
            return (c => Pair<byte[], OpenConnection>.Create(c.GetData(), c));
        }

        public static Func<OpenConnection, IPair<Unit, ClosedConnection>> Close()
        {
            return State.Modify<OpenConnection, ClosedConnection>(c => c.Close());
        }

        public static Func<ClosedConnection, IPair<Unit, DisposedConnection>> Dispose()
        {
            return State.Modify<ClosedConnection, DisposedConnection>(c => c.Dispose());
        }

        public static A RunWithConnection<A>(this Func<ClosedConnection, IPair<A, DisposedConnection>> state, TcpClient client)
        {
            return state.Eval(new ClosedConnection(client));
        }
    }
}
