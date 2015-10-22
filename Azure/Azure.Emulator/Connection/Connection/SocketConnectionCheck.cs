using System;
using System.Net.Sockets;

namespace Azure.Connection.Connection
{
    internal class SocketConnectionCheck
    {
        private static string[] _mConnectionStorage;
        private static string _mLastIpBlocked;

        internal static bool CheckConnection(Socket sock, int maxIpConnectionCount, bool antiDDosStatus)
        {
            if (!antiDDosStatus)
                return true;

            string iP = sock.RemoteEndPoint.ToString().Split(':')[0];
            if (iP == _mLastIpBlocked)
            {
                iP = null;
                return false;
            }
            if ((GetConnectionAmount(iP) > maxIpConnectionCount))
            {
                Out.WriteLine(iP + " was banned by Anti-DDoS system.", "Azure.TcpAntiDDoS", ConsoleColor.Blue);
                _mLastIpBlocked = iP;
                iP = null;
                return false;
            }
            int freeConnectionId = GetFreeConnectionId();
            if (freeConnectionId < 0)
                return false;
            _mConnectionStorage[freeConnectionId] = iP;
            iP = null;
            return true;
        }

        internal static void FreeConnection(string ip)
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == ip)
                    _mConnectionStorage[i] = null;
        }

        private static int GetConnectionAmount(string ip)
        {
            int count = 0;
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == ip)
                    count++;
            return count;
        }

        private static int GetFreeConnectionId()
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == null)
                    return i;
            return -1;
        }

        internal static void SetupTcpAuthorization(int connectionCount)
        {
            _mConnectionStorage = new string[connectionCount];
        }
    }
}