// -----------------------------------------------------------------------
//  <copyright file = "FiCommunicator.cs" company = "IGT">
//      Copyright (c) 2018 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Utp.Framework
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// Handles all communications with the test FI.
    /// </summary>
    public class FiCommunicator
    {
        /// <summary>
        /// Port used to create the socket connection with the FI to send/receive PAD commands.
        /// </summary>
        private const int fiPort = 5741;

        /// <summary>
        /// PAD command to get whether UTP is currently enabled or disabled.
        /// </summary>
        private const string fiUtpEnabledCmd = "get utp status\r\n";

        /// <summary>
        /// PAD response when UTP is enabled.
        /// </summary>
        private const string fiResponseUtpEnabled = "Enabled\r\n";

        /// <summary>
        /// PAD reponse when UTP is disabled.
        /// </summary>
        private const string fiResponseUtpDisabled = "Disabled\r\n";

        public enum UtpFiStatus
        {
            /// <summary>
            /// This is used when the FI reports that UTP should be enabled.
            /// </summary>
            Enabled,

            /// <summary>
            /// This is used when UTP was explicitly disabled via the test FI.
            /// </summary>
            Disabled,

            /// <summary>
            /// This signifies that an error occurred while attempting to communicate with the test FI.
            /// </summary>
            Error
        }

        /// <summary>
        /// Check with the test FI to determine if UTP should be enabled.
        /// </summary>
        /// <returns>UTP's enabled status according to the test FI.</returns>
        public UtpFiStatus GetUtpEnabled()
        {
            var bytes = new byte[1024];
            try
            {
                IPAddress ipAddress = null;

                //  It's possible some 4.0+ brain boxes have multiple NICs, resulting in multiple IPv4 addresses
                // ReSharper disable once LoopCanBeConvertedToQuery //  Disabling for the sake of clarity
                foreach (var address in Dns.GetHostEntry("localhost").AddressList)
                {
                    //  This check verifies we have an IPv4 address
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    //  We only want the loopback address
                    if (!IPAddress.IsLoopback(address))
                    {
                        continue;
                    }

                    //  We've found the IPv4 loopback address
                    ipAddress = address;
                    break;
                }

                //  If an IPv4 loopback address wasn't found, then we can't proceed
                if (ipAddress == null)
                    return UtpFiStatus.Error;

                var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //  Connect & get the initial response from the FI, containing versioning info
                    sender.Connect(new IPEndPoint(ipAddress, fiPort));
                    sender.Receive(bytes);

                    //  Send the pad command to get UTP enabled status
                    sender.Send(Encoding.ASCII.GetBytes(fiUtpEnabledCmd));
                    var bytesRec = sender.Receive(bytes);
                    var response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    //  NOTE:
                    //  It's possible that the response may include extra noise from unrelated events, such as game play, etc.
                    //  We deal with this by checking if the response Contains one of the strings we're looking for.  A direct
                    //  comparison using the == operator seems more secure, since a response of 'NotEnabled\r\n' actually returns
                    //  true when using Contains.  However, we can play it safe by checking for 'Disabled' first.  Also, it's not
                    //  really a security issue at this point since we know an FI is installed.
                    if (response.Contains(fiResponseUtpDisabled))
                    {
                        return UtpFiStatus.Disabled;
                    }

                    if (response.Contains(fiResponseUtpEnabled))
                    {
                        return UtpFiStatus.Enabled;
                    }

                    return UtpFiStatus.Error;
                }
                finally
                {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }
            catch
            {
                return UtpFiStatus.Error;
            }
        }
    }
}