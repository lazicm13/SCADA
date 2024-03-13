﻿using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;
            byte[] array = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, array, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, array, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, array, 4, 2);
            array[6] = CommandParameters.UnitId;
            array[7] = CommandParameters.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.StartAddress)), 0, array, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Quantity)), 0, array, 10, 2);

            return array;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> respDict = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }
            else
            {
                ushort address = mrcp.StartAddress;
                ushort value;

                for (int i = 0; i < response[8]; i = i + 2)
                {
                    value = BitConverter.ToUInt16(response, (i + 9));
                    value = (ushort)IPAddress.NetworkToHostOrder((short)value);

                    respDict.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);
                    address++;
                }
            }
            return respDict;
        }
    }
}