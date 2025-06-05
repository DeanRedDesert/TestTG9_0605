using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Foundation.Transport")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Foundation.Transport")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1be7e282-c729-4096-b2d7-2c41d9a414db")]

// Allow the unit tests to see internals for this assembly
[assembly: InternalsVisibleTo("IGT.Game.Core.Communication.Foundation.Transport.Tests")]

// CsiSocketTransport extends SocketTransport, so it needs to see internals also
[assembly: InternalsVisibleTo("IGT.Game.Core.Communication.Cabinet.CsiTransport")]
[assembly: InternalsVisibleTo("IGT.Game.Core.Communication.Cabinet.CsiTransport.Tests")]

// TODO: remove this!
[assembly: InternalsVisibleTo("Server")]
