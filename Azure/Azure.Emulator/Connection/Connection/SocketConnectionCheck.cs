#region

using System;
using System.Net.Sockets;

#endregion

namespace Azure.Connection.Connection
{
    internal class SocketConnectionCheck
    {
        private static string[] _mConnectionStorage;
        private static string _mLastIpBlocked;

        internal static bool CheckConnection(Socket Sock, int MaxIpConnectionCount, bool AntiDDosStatus)
        {
            if (!AntiDDosStatus)
                return true;

            string iP = Sock.RemoteEndPoint.ToString().Split(':')[0];
            if (iP == _mLastIpBlocked)
            {
                iP = null;
                return false;
            }
            if ((GetConnectionAmount(iP) > MaxIpConnectionCount))
            {
                Out.WriteLine(iP + " was banned by Anti-DDoS system.", "Azure.TcpAntiDDoS", ConsoleColor.Blue);
                _mLastIpBlocked = iP;
                iP = null;
                return false;
            }
            int freeConnectionID = GetFreeConnectionID();
            if (freeConnectionID < 0)
                return false;
            _mConnectionStorage[freeConnectionID] = iP;
            iP = null;
            return true;
        }

        internal static void FreeConnection(string IP)
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == IP)
                    _mConnectionStorage[i] = null;
        }

        private static int GetConnectionAmount(string IP)
        {
            int count = 0;
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == IP)
                    count++;
            return count;
        }

        private static int GetFreeConnectionID()
        {
            for (int i = 0; i < _mConnectionStorage.Length; i++)
                if (_mConnectionStorage[i] == null)
                    return i;
            return -1;
        }

        internal static void SetupTcpAuthorization(int ConnectionCount)
        {
            _mConnectionStorage = new string[ConnectionCount];
        }
    }
}

