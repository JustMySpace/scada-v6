﻿/*
 * Copyright 2020 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaCommEngine
 * Summary  : Represents a communication line
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2006
 * Modified : 2020
 */

using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Drivers;
using Scada.Data.Models;
using Scada.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Scada.Comm.Engine
{
    /// <summary>
    /// Represents a communication line.
    /// <para>Представляет линию связи.</para>
    /// </summary>
    internal class CommLine : ILineContext
    {
        private readonly CoreLogic coreLogic;       // the Communicator logic instance
        private readonly string infoFileName;       // the full file name to write communication line information

        private Thread thread;                      // the working thread of the communication line
        private volatile bool terminated;           // necessary to stop the thread
        private volatile ServiceStatus lineStatus;  // the current communication line status
        private int lastInfoLength;                 // the last info text length


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        private CommLine(LineConfig lineConfig, CoreLogic coreLogic)
        {
            LineConfig = lineConfig ?? throw new ArgumentNullException(nameof(lineConfig));
            this.coreLogic = coreLogic ?? throw new ArgumentNullException(nameof(coreLogic));
            infoFileName = Path.Combine(coreLogic.AppDirs.LogDir, CommUtils.GetLineLogFileName(CommLineNum, ".txt"));

            thread = null;
            terminated = false;
            lineStatus = ServiceStatus.Undefined;
            lastInfoLength = 0;

            Channel = null;
            Devices = new List<DeviceLogic>();
            Title = CommUtils.GetLineTitle(CommLineNum, LineConfig.Name);
            SharedData = null;
            Log = new LogFile(LogFormat.Full)
            {
                FileName = Path.Combine(coreLogic.AppDirs.LogDir, CommUtils.GetLineLogFileName(CommLineNum, ".log")),
                Capacity = coreLogic.Config.GeneralOptions.MaxLogSize
            };
        }


        /// <summary>
        /// Gets or sets the communication channel.
        /// </summary>
        private ChannelLogic Channel { get; set; }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        private List<DeviceLogic> Devices { get; }

        /// <summary>
        /// Gets the communication line configuration.
        /// </summary>
        public LineConfig LineConfig { get; }

        /// <summary>
        /// Gets the communication line number.
        /// </summary>
        public int CommLineNum
        {
            get
            {
                return LineConfig.CommLineNum;
            }
        }

        /// <summary>
        /// Gets the communication line title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the communication line log.
        /// </summary>
        public ILog Log { get; }

        /// <summary>
        /// Gets the shared data of the communication line.
        /// </summary>
        public IDictionary<string, object> SharedData { get; private set; }

        /// <summary>
        /// Gets the current communication line status.
        /// </summary>
        public ServiceStatus LineStatus
        {
            get
            {
                return lineStatus;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the communication line is terminated.
        /// </summary>
        public bool IsTerminated
        {
            get
            {
                return lineStatus == ServiceStatus.Terminated;
            }
        }


        /// <summary>
        /// Binds the communication line to the configuration database.
        /// </summary>
        private void BindToBase(BaseDataSet baseDataSet)
        {
            foreach (DeviceLogic deviceLogic in Devices)
            {
                deviceLogic.Bind(baseDataSet);
            }
        }

        /// <summary>
        /// Prepares the communication line for start.
        /// </summary>
        private void Prepare()
        {
            terminated = false;
            lineStatus = ServiceStatus.Starting;
            SharedData = new ConcurrentDictionary<string, object>();
            WriteInfo();
        }

        /// <summary>
        /// Operating cycle running in a separate thread.
        /// </summary>
        private void Execute()
        {
            try
            {
                //Devices.ForEach(d => d.OnCommLineStart());

                // bind to the configuration database
                //if (lineConfig.IsBound && coreLogic.BaseDataSet != null)
                //    commLine.BindToBase(coreLogic.BaseDataSet);

                lineStatus = ServiceStatus.Normal;

                while (!terminated)
                {
                    Thread.Sleep(ScadaUtils.ThreadDelay);
                }
            }
            finally
            {
                lineStatus = ServiceStatus.Terminated;
                WriteInfo();

                Log.WriteAction(Locale.IsRussian ?
                    "Линия связи {0} остановлена" :
                    "Communication line {0} is stopped", Title);
                Log.WriteBreak();
            }
        }
        
        /// <summary>
        /// Writes application information to the file.
        /// </summary>
        private void WriteInfo()
        {
            try
            {
                // prepare information
                StringBuilder sb = new StringBuilder((int)(lastInfoLength * 1.1));
                sb
                    .AppendLine(Title)
                    .Append('-', Title.Length).AppendLine();

                if (Locale.IsRussian)
                {
                    sb.Append("Статус : ").AppendLine(lineStatus.ToString(true));
                }
                else
                {
                    sb.Append("Status : ").AppendLine(lineStatus.ToString(false));
                }

                lastInfoLength = sb.Length;

                // write to file
                using (StreamWriter writer = new StreamWriter(infoFileName, false, Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex, Locale.IsRussian ?
                    "Ошибка при записи в файл информации о работе линии связи" :
                    "Error writing communication line information to the file");
            }
        }


        /// <summary>
        /// Starts the communication line.
        /// </summary>
        public bool Start()
        {
            try
            {
                if (thread == null)
                {
                    Log.WriteBreak();
                    Log.WriteAction(Locale.IsRussian ? 
                        "Запуск линии связи {0}" :
                        "Start communication line {0}", Title);
                    Prepare();
                    thread = new Thread(Execute);
                    thread.Start();
                }
                else
                {
                    Log.WriteAction(Locale.IsRussian ?
                        "Линия связи {0} уже запущена" :
                        "Communication line {0} is already started", Title);
                }

                return thread != null;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex, Locale.IsRussian ?
                    "Ошибка при запуске линии связи {0}" :
                    "Error starting communication line {0}", Title);
                return false;
            }
            finally
            {
                if (thread == null)
                {
                    lineStatus = ServiceStatus.Error;
                    WriteInfo();
                }
            }
        }

        /// <summary>
        /// Begins termination process of the communication line.
        /// </summary>
        public void Terminate()
        {
            terminated = true;
        }

        /// <summary>
        /// Sends the telecontrol command to the current communication line.
        /// </summary>
        public void SendCommand(TeleCommand cmd)
        {

        }

        /// <summary>
        /// Creates a communication line, communication channel and devices.
        /// </summary>
        public static CommLine Create(LineConfig lineConfig, CoreLogic coreLogic, DriverHolder driverHolder)
        {
            // create communication line
            CommLine commLine = new CommLine(lineConfig, coreLogic);

            // create communication channel
            if (!string.IsNullOrEmpty(lineConfig.Channel.TypeName))
            {
                if (driverHolder.GetDriver(lineConfig.Channel.Driver, out DriverLogic driverLogic))
                {
                    commLine.Channel = driverLogic.CreateChannel(commLine, lineConfig.Channel);
                }
                else
                {
                    throw new ScadaException(Locale.IsRussian ?
                        "Драйвер для создания канала связи не найден." :
                        "Driver for creating communication channel not found.");
                }
            }

            // create devices
            foreach (DeviceConfig deviceConfig in lineConfig.DevicePolling)
            {
                if (driverHolder.GetDriver(deviceConfig.Driver, out DriverLogic driverLogic))
                {
                    DeviceLogic deviceLogic = driverLogic.CreateDevice(commLine, deviceConfig);
                    commLine.Devices.Add(deviceLogic);
                }
            }

            return commLine;
        }
    }
}
