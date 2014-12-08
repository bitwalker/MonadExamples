using System;
using System.Net;
using System.Net.Sockets;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    /// <summary>
    /// This class represents a basic network connection. It's implementations
    /// define specific operations that can be performed on that connection.
    /// </summary>
    public abstract class ClientConnection
    {
        protected readonly TcpClient _client;
        protected NetworkStream _stream;

        /// <summary>
        /// Creates a new TCP connection using the given hostname and port
        /// </summary>
        /// <param name="hostname">The hostname to connect to</param>
        /// <param name="port">The port to connect to</param>
        protected ClientConnection(string hostname, int port)
        {
            _client = new TcpClient(hostname, port);
        }

        /// <summary>
        /// Creates an instance which wraps an existing TcpClient
        /// </summary>
        /// <param name="client">The TcpClient to wrap</param>
        protected ClientConnection(TcpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Creates an instance which wraps an existing TcpClient and an open NetworkStream
        /// </summary>
        /// <param name="client">The TcpClient to wrap</param>
        /// <param name="stream">The NetworkStream to wrap</param>
        protected ClientConnection(TcpClient client, NetworkStream stream)
        {
            _client = client;
            _stream = stream;
        }
    }

    /// <summary>
    /// Defines the behavior and actions which can be taken on an open network connection.
    /// </summary>
    public class OpenConnection : ClientConnection
    {
        public OpenConnection(TcpClient client) : base(client, client.GetStream()) { }
        public OpenConnection(TcpClient client, NetworkStream stream) : base(client, stream) { }

        /// <summary>
        /// An open connection can be closed.
        /// </summary>
        /// <returns>ClosedConnection</returns>
        public ClosedConnection Close()
        {
            _stream.Close();
            return new ClosedConnection(_client);
        }

        /// <summary>
        /// An open connection can read data from the network stream
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] GetData()
        {
            var data = new byte[4096];
            var bytesRead = 0;

            try
            {
                // blocking operation until client receives message
                bytesRead = _stream.Read(data, 0, 4096);
            }
            catch (Exception)
            {
                // Swallow it
            }

            return data;
        }
    }

    /// <summary>
    /// Defines the behavior and actions which can be taken on a closed network connection.
    /// </summary>
    public class ClosedConnection : ClientConnection
    {
        public ClosedConnection(string hostname, int port) : base(hostname, port) { }
        public ClosedConnection(TcpClient client) : base(client) { }

        /// <summary>
        /// A closed connection can be opened.
        /// </summary>
        /// <returns>OpenConnection</returns>
        public OpenConnection Open() { return new OpenConnection(_client); }
        /// <summary>
        /// A closed connection can be disposed.
        /// </summary>
        /// <returns>DisposedConnection</returns>
        public DisposedConnection Dispose()
        {
            _stream.Dispose();
            _client.Close();
            return new DisposedConnection(null);
        }
    }

    /// <summary>
    /// Defines the behavior and actions which can be taken on a disposed network connection.
    /// Which just so happens to be exactly nothing.
    /// </summary>
    public class DisposedConnection : ClientConnection
    {
        public DisposedConnection(TcpClient client) : base(client) { }
    }

    /// <summary>
    /// Extensions for generating functions which transform ClientConnections from one state to another.
    /// </summary>
    public static class Connections
    {
        /// <summary>
        /// Given a ClosedConnection, generate a function which returns a Pair where the new
        /// state is an OpenConnection.
        /// </summary>
        /// <returns>IPair of type Unit/OpenConnection</returns>
        public static Func<ClosedConnection, IPair<Unit, OpenConnection>> Open()
        {
            return State.Modify<ClosedConnection, OpenConnection>(c => c.Open());
        }

        /// <summary>
        /// Given an OpenConnection, generate a function which returns a Pair where the
        /// left contains data read from the connection, and the right contains the modified
        /// OpenConnection state.
        /// </summary>
        /// <returns>IPair of type byte[]/OpenConnection</returns>
        public static Func<OpenConnection, IPair<byte[], OpenConnection>> GetData()
        {
            return (c => new Pair<byte[], OpenConnection>(c.GetData(), c));
        }

        /// <summary>
        /// Given an OpenConnection, generate a function which returns a Pair where the new
        /// state is a ClosedConnection.
        /// </summary>
        /// <returns></returns>
        public static Func<OpenConnection, IPair<Unit, ClosedConnection>> Close()
        {
            return State.Modify<OpenConnection, ClosedConnection>(c => c.Close());
        }

        /// <summary>
        /// Given a ClosedConnection, generate a function which returns a Pair where the new
        /// state is a DisposedConnection.
        /// </summary>
        /// <returns></returns>
        public static Func<ClosedConnection, IPair<Unit, DisposedConnection>> Dispose()
        {
            return State.Modify<ClosedConnection, DisposedConnection>(c => c.Dispose());
        }

        /// <summary>
        /// Given a TcpClient, and a function which takes a ClosedConnection and returns a Pair where the
        /// left represents the type of data being returned, and the right represents a DisposedConnection,
        /// execute that function and return the data contained in IPair.Left
        /// </summary>
        /// <typeparam name="A">The type of the data returned</typeparam>
        /// <param name="state">The state transformation function, which starts with a ClosedConnection, and must end with a DisposedConnection</param>
        /// <param name="client">The TcpClient instance to use for this run</param>
        /// <returns>The data extracted from the connection, transformed to type A</returns>
        public static A RunWithConnection<A>(this Func<ClosedConnection, IPair<A, DisposedConnection>> state, TcpClient client)
        {
            return state.Eval(new ClosedConnection(client));
        }

        /// <summary>
        /// Given a hostname and port, and a function which takes a ClosedConnection and returns a Pair where the
        /// left represents the type of data being returned, and the right represents a DisposedConnection,
        /// execute that function and return the data contained in IPair.Left
        /// </summary>
        /// <typeparam name="A">The type of the data returned</typeparam>
        /// <param name="state">The state transformation function, which starts with a ClosedConnection, and must end with a DisposedConnection</param>
        /// <param name="client">The TcpClient instance to use for this run</param>
        /// <returns>The data extracted from the connection, transformed to type A</returns>
        public static A RunWithConnection<A>(this Func<ClosedConnection, IPair<A, DisposedConnection>> state, string hostname, int port)
        {
            return state.Eval(new ClosedConnection(hostname, port));
        }
    }
}
