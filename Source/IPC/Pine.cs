// this is a re-implementation of pine in C# to avoid using native libraries
// https://projects.govanify.com/govanify/pine

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Godot;

namespace KH2MusicTool.Source.IPC;

public class PCSX2() : IPCBase(28011, "pcsx2");

public abstract class IPCBase
{
    public const int PacketSizeSize = sizeof(uint);
    public const int PacketTypeSize = sizeof(byte);
    public const int PacketHeaderSize = PacketSizeSize + PacketTypeSize;
    public const int PacketAddressSize = sizeof(uint);
    public const int PacketHeaderWithAddressSize = PacketHeaderSize + PacketAddressSize;
    public enum IPCCommand : byte
    {
        MsgRead8 = 0,           /**< Read 8 bit value to memory. */
        MsgRead16 = 1,          /**< Read 16 bit value to memory. */
        MsgRead32 = 2,          /**< Read 32 bit value to memory. */
        MsgRead64 = 3,          /**< Read 64 bit value to memory. */
        MsgWrite8 = 4,          /**< Write 8 bit value to memory. */
        MsgWrite16 = 5,         /**< Write 16 bit value to memory. */
        MsgWrite32 = 6,         /**< Write 32 bit value to memory. */
        MsgWrite64 = 7,         /**< Write 64 bit value to memory. */
        MsgVersion = 8,         /**< Returns the emulator version. */
        MsgSaveState = 9,       /**< Saves a savestate. */
        MsgLoadState = 0xA,     /**< Loads a savestate. */
        MsgTitle = 0xB,         /**< Returns the game title. */
        MsgID = 0xC,            /**< Returns the game ID. */
        MsgUUID = 0xD,          /**< Returns the game UUID. */
        MsgGameVersion = 0xE,   /**< Returns the game verion. */
        MsgStatus = 0xF,        /**< Returns the emulator status. */
        MsgUnimplemented = 0xFF /**< Unimplemented IPC message. */
    }

    public enum EmuStatus : uint
    {
        Running = 0,  /**< Game is running */
        Paused = 1,   /**< Game is paused */
        Shutdown = 2, /**< Game is shutdown */
    }
    
    protected ushort Slot;
    protected string EmulatorName;
    protected Socket Socket;
    protected EndPoint SocketEndpoint;

    public bool Connected => Socket.Connected;

    protected IPCBase(ushort slot, string emulatorName)
    {
        Slot = slot;
        EmulatorName = emulatorName;
        
        if (OperatingSystem.IsWindows())
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //GD.Print("endpoint is ip");
            SocketEndpoint = new IPEndPoint(IPAddress.Loopback, Slot);
        }
        else
        {
            Socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            var env = System.Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "tmp";
            var path = Path.Combine(env, $"{EmulatorName}.sock");
            SocketEndpoint = new UnixDomainSocketEndPoint(path);
        }

        //Socket.ReceiveTimeout = 100;
        
        InitSocket();
    }

    public void Write8(uint address, byte value)
    {
        const int totalSize = PacketHeaderWithAddressSize + sizeof(byte);
        
        Span<byte> buffer = stackalloc byte[totalSize];
        FormatBeginning(ref buffer, IPCCommand.MsgWrite8, address, totalSize);
        buffer[PacketHeaderWithAddressSize + 1] = value;
        Span<byte> returnBuffer = stackalloc byte[PacketHeaderSize];
        SendCommand(buffer, ref returnBuffer);
    }
    public void Write16(uint address, ushort value)
    {
        const int totalSize = PacketHeaderWithAddressSize + sizeof(ushort);
        
        Span<byte> buffer = stackalloc byte[totalSize];
        FormatBeginning(ref buffer, IPCCommand.MsgWrite16, address, totalSize);
        Span<byte> byteBuffer = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(byteBuffer, value);
        for (var i = 0; i < byteBuffer.Length; i++) buffer[PacketHeaderWithAddressSize + i] = byteBuffer[i];
        Span<byte> returnBuffer = stackalloc byte[PacketHeaderSize];
        SendCommand(buffer, ref returnBuffer);
    }
    public void Write32(uint address, uint value)
    {
        const int totalSize = PacketHeaderWithAddressSize + sizeof(uint);
        
        Span<byte> buffer = stackalloc byte[totalSize];
        FormatBeginning(ref buffer, IPCCommand.MsgWrite32, address, totalSize);
        Span<byte> byteBuffer = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(byteBuffer, value);
        for (var i = 0; i < byteBuffer.Length; i++) buffer[PacketHeaderWithAddressSize + i] = byteBuffer[i];
        Span<byte> returnBuffer = stackalloc byte[PacketHeaderSize];
        SendCommand(buffer, ref returnBuffer);
    }
    public void Write64(uint address, ulong value)
    {
        const int totalSize = PacketHeaderWithAddressSize + sizeof(ulong);
        
        Span<byte> buffer = stackalloc byte[totalSize];
        FormatBeginning(ref buffer, IPCCommand.MsgWrite64, address, totalSize);
        Span<byte> byteBuffer = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(byteBuffer, value);
        for (var i = 0; i < byteBuffer.Length; i++) buffer[PacketHeaderWithAddressSize + i] = byteBuffer[i];
        Span<byte> returnBuffer = stackalloc byte[PacketHeaderSize];
        SendCommand(buffer, ref returnBuffer);
    }
    
    public byte Get8(uint address)
    {
        Span<byte> buffer = stackalloc byte[PacketHeaderSize + sizeof(byte)];
        
        if (!SendCommand(FormatBeginning(IPCCommand.MsgRead8, address, 9), ref buffer)) return byte.MaxValue;
        
        var bytes = buffer[5..(5 + sizeof(byte))];
        return bytes[0];
    }
    public ushort Get16(uint address)
    {
        Span<byte> buffer = stackalloc byte[PacketHeaderSize + sizeof(ushort)];
        
        if (!SendCommand(FormatBeginning(IPCCommand.MsgRead16, address, 9), ref buffer)) return ushort.MaxValue;
        
        var bytes = buffer[5..(5 + sizeof(ushort))];
        return BitConverter.ToUInt16(bytes);
    }
    public uint Get32(uint address)
    {
        Span<byte> buffer = stackalloc byte[PacketHeaderSize + sizeof(uint)];
        
        if (!SendCommand(FormatBeginning(IPCCommand.MsgRead32, address, 9), ref buffer)) return uint.MaxValue;
        
        //GD.Print(string.Concat(buffer.ToArray().Select(i => i.ToString("x2"))));
        
        var bytes = buffer[5..(5 + sizeof(uint))];
        return BitConverter.ToUInt32(bytes);
    }
    public ulong Get64(uint address)
    {
        Span<byte> buffer = stackalloc byte[PacketHeaderSize + sizeof(ulong)];
        
        if (!SendCommand(FormatBeginning(IPCCommand.MsgRead64, address, 9), ref buffer)) return ulong.MaxValue;
        
        var bytes = buffer[5..(5 + sizeof(ulong))];
        return BitConverter.ToUInt64(bytes);
    }
    
    public bool SendCommand(Span<byte> command, ref Span<byte> buffer)
    {
        if (!Socket.Connected) InitSocket();
        try
        {
            Socket.Send(command);
        }
        catch
        {
            return false;
        }
        
        try
        {
            Socket.Receive(buffer);
        }
        catch
        {
            return false;
        }
        return true;
    }
    public Span<byte> FormatBeginning(IPCCommand command, uint address, uint size)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint) + sizeof(uint) + 1];
        FormatBeginning(ref buffer, command, address, size);
        return buffer.ToArray();
    }
    public void FormatBeginning(ref Span<byte> buffer, IPCCommand command, uint address, uint size)
    {
        var addrBytes = BitConverter.GetBytes(address);
        var sizeBytes = BitConverter.GetBytes(size);
        var index = 0;
        for (var i = 0; i < sizeof(uint); i++) buffer[index++] = sizeBytes[i];
        buffer[index++] = (byte)command;
        for (var i = 0; i < sizeof(uint); i++) buffer[index++] = addrBytes[i];
    }
    public bool InitSocket()
    {
        try
        {
            Socket.Connect(SocketEndpoint);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}