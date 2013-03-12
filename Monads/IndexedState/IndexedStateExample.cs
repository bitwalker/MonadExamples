using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Unit = System.Reactive.Unit;

namespace Monads.IndexedState
{
    public class IndexedStateExample
    {
        public static void Main(string[] args) {
            var client  = new TcpClient("localhost", 8080);
            var data    = ReceiveData(client);
            var message = Encoding.ASCII.GetString(data);
            Console.WriteLine(message);
        }

        public static byte[] ReceiveData(TcpClient client)
        {
            // By using the Indexed State monad, we can wrap type safety around our operations, this allows
            // the compiler to act as a catch-all for easy mistakes like opening an open connection, reading
            // data from a closed connection, etc.

            // In this case, we are attempting to read data from a connection stream:

            // RunWithConnection starts off by creating a ClosedConnection with the provided TcpClient instance
            return (
                // Open opens that connection and provides an instance of OpenConnection
                from o    in Connections.Open()
                // GetData uses the OpenConnection and retreives the byte[] data from the connection stream
                from data in Connections.GetData()
                // Close closes the OpenConnection and provides an instance of ClosedConnection
                from c    in Connections.Close()
                // Dispose disposes the ClosedConnection and provides an instance of DisposedConnection
                from d    in Connections.Dispose()
                select data
            ).RunWithConnection(client);
        }
    }
}