﻿
namespace Tx.Network
{
    using System;
    using System.Net;
    using System.IO;
    using System.Collections.Generic;
    using Tx.Network.Snmp;


    /// <summary>
    /// Extentions to Byte[] to: 
    ///     - read bits at specified offsets from either a Byte or a network order UShort
    ///     - read a single Byte or network order UShort
    ///     - read an IPv4 Address
    /// </summary>
    public static class ByteArrayExtentions
    {
        #region Byte[] Extentions
        public static byte ReadBits(this byte[] bytes, int BufferOffset, int BitPosition, int BitLength)
        {
            return bytes[BufferOffset].ReadBits(BitPosition, BitLength);
        }
        public static byte ReadBits(this byte bytes, int BitPosition, int BitLength)
        {
            var bitShift = 8 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 8 for byte output type.");
            }
            return (byte)(((0xff >> (BitPosition)) & bytes) >> bitShift);
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int BufferOffset, int BitPosition, int BitLength)
        {
            var bitShift = 16 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new ArgumentOutOfRangeException("BitPostion + BitLength greater than 16 for ushort output type.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(((0xffff >> BitPosition) & BitConverter.ToUInt16(bytes, BufferOffset) >> bitShift));
        }
        public static ushort ReadNetOrderUShort(this byte[] bytes, int BufferOffset)
        {
            if (bytes.Length - BufferOffset < 2)
            {
                throw new ArgumentOutOfRangeException("Buffer offset overflows size of byte array.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, BufferOffset));
        }
        public static IPAddress ReadIpAddress(this byte[] bytes, int BufferOffset)
        {
            var IpBytes = new byte[4];
            Array.Copy(bytes, BufferOffset, IpBytes, 0, 4);
            return new IPAddress(IpBytes);
        }
        #endregion
    }

    /// <summary>
    /// Extentions to BinaryReader to: 
    ///     - read bits at specified offsets from either a Byte or a network order UShort
    ///     - read a single Byte or network order UShort
    ///     - read an IPv4 Address
    ///     - look at the current byte without moving the the position in the stream
    /// </summary>
    public static class BinaryReaderExtentions
    {
        #region BinaryReader Extentions
        public static byte PeekByte(this BinaryReader bytes)
        {
            var pos = bytes.BaseStream.Position;
            var b = bytes.ReadByte();
            bytes.BaseStream.Seek(pos, 0);
            return b;
        }
        public static byte ReadBits(this BinaryReader bytes, int BitPosition, int BitLength, bool Advance = false)
        {
            if (Advance) return bytes.ReadByte().ReadBits(BitPosition, BitLength);
            return bytes.PeekByte().ReadBits(BitPosition, BitLength);
        }
        public static ushort ReadNetOrderUShort(this BinaryReader bytes)
        {
            return (ushort)IPAddress.NetworkToHostOrder(bytes.ReadInt16());
        }
        public static ushort ReadNetOrderUShort(this BinaryReader bytes, int BitPosition, int BitLength)
        {
            var bitShift = 16 - BitPosition - BitLength;
            if (bitShift < 0)
            {
                throw new  ArgumentOutOfRangeException("BitPostion + BitLength greater than 16 for ushort output type.");
            }
            return (ushort)IPAddress.NetworkToHostOrder(((0xffff >> BitPosition)) & bytes.ReadUInt16() >> bitShift);
        }
        public static IPAddress ReadIpAddress(this BinaryReader bytes)
        {
            return new IPAddress(bytes.ReadBytes(4));
        }
        #endregion
    }


    /// <summary>
    /// class to provide extension methhods to calculate Enterprises
    /// </summary>
    public static class Enterprises
    {
        /// <summary>
        /// Enterprise names as per
        /// http://www.iana.org/assignments/enterprise-numbers/enterprise-numbers
        /// </summary>
        private static readonly IDictionary<uint, string> names =
            new Dictionary<uint, string>()
            {
                {9, "Cisco"},
                {21296, "Infinera"},
                {3780, "Level3"},
                {6027, "Force10"},
                {30065, "Arista"},
                {2636, "Juniper"},
                {8072, "net-snmp"},
            };

        /// <summary>
        /// The prefix
        /// </summary>
        private static readonly ObjectIdentifier prefixOid = new ObjectIdentifier("1.3.6.1.4.1");

        /// <summary>
        /// Method to return Enterprise name.
        /// </summary>
        /// <param name="oid">The ObjectIdentifier as string.</param>
        /// <returns>Enterprise name</returns>
        public static string GetEnterpriseName(this string oid)
        {
            if (string.IsNullOrEmpty(oid) || oid.Length < 6)
            {
                return null;
            }

            return GetEnterpriseName(new ObjectIdentifier(oid));
        }

        /// <summary>
        /// Method to return Enterprise name.
        /// </summary>
        /// <param name="oid">The ObjectIdentifier object.</param>
        /// <returns>Enterprise name</returns>
        public static string GetEnterpriseName(this ObjectIdentifier oid)
        {
            if (oid.IsSubOid(prefixOid) && oid.Oids.Count > 6)
            {
                string enterprise;
                if (!names.TryGetValue(oid.Oids[6], out enterprise))
                {
                    enterprise = "Unknown (" + oid.Oids[6] + ")";
                }

                return enterprise;
            }

            return null;
        }
    }
}

