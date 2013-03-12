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

        protected ClientConnection(TcpClient client) { 
            _client = client;
            _stream = client.GetStream ();
        }

        protected ClientConnection(TcpClient client, NetworkStream stream) {
            _client = client;
            _stream = stream;
        }
    }

    public class OpenConnection : ClientConnection
    {
        public OpenConnection(TcpClient client, NetworkStream stream) : base(client, stream) { }

        public ClosedConnection Close()    { return new ClosedConnection(_client, _stream); }
        public byte[]           GetData()  { 
            var data      = new byte[4096];
            var bytesRead = 0;

            try {
                // blocking operation until client receives message
                bytesRead = _stream.Read (data, 0, 4096);
            }
            catch (Exception ex) {
                // Swallow it
            }

            return data;
        }
    }

    public class ClosedConnection : ClientConnection
    {
        public ClosedConnection(TcpClient client, NetworkStream stream) : base(client, stream) {
            _client.Close ();
        }
    }

    public static class Connections
    {
        public static State<ClosedConnection, OpenConnection, Unit> Open() {
            return State.Modify(c => c.Open());
        }

        public static State<OpenConnection, OpenConnection, byte[]> GetData() {
            return (c => Pair.Create(c.GetData(), c));
        }

        public static State<OpenConnection, ClosedConnection, Unit> Close() {
            return State.Modify(c => c.Close());
        }

        public static A RunWithConnection<A>(this State<OpenConnection, ClosedConnection, A> state, TcpClient client)
        {
            state.Eval(new OpenConnection(client));
        }

    }

    public class IndexedStateExample
    {
        public static void Main(string[] args) {
            var client = new TcpClient("localhost", 8080);
            var data   = ReceiveData(client);
        }

        public static byte[] ReceiveData(TcpClient client)
        {
            return (
                from   open    in Connections.Open()
                from   data    in Connections.GetData()
                from   close   in Connections.Close()
                select data
            ).RunWithConnection(client);
        }
    }
}