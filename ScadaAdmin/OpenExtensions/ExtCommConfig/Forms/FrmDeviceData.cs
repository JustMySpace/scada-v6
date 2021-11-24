﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Admin.Lang;
using Scada.Agent;
using Scada.Comm;
using Scada.Comm.Config;
using Scada.Forms;
using Scada.Protocol;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinControl;

namespace Scada.Admin.Extensions.ExtCommConfig.Forms
{
    /// <summary>
    /// Represents a form for displaying device data.
    /// <para>Представляет форму для отображения данных КП.</para>
    /// </summary>
    public partial class FrmDeviceData : Form, IChildForm
    {
        private readonly IAdminContext adminContext; // the Administrator context
        private readonly DeviceConfig deviceConfig;  // the device configuration
        private readonly RemoteLogBox dataBox;       // updates device data
        //private FrmDeviceCommand frmDeviceCommand;   // the form to send commands


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        private FrmDeviceData()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmDeviceData(IAdminContext adminContext, DeviceConfig deviceConfig)
            : this()
        {
            this.adminContext = adminContext ?? throw new ArgumentNullException(nameof(adminContext));
            this.deviceConfig = deviceConfig ?? throw new ArgumentNullException(nameof(deviceConfig));
            dataBox = new RemoteLogBox(lbDeviceData) { FullLogView = true };
            //frmDeviceCommand = null;
        }


        /// <summary>
        /// Gets or sets the object associated with the form.
        /// </summary>
        public ChildFormTag ChildFormTag { get; set; }


        /// <summary>
        /// Initializes the log box.
        /// </summary>
        private void InitLogBox()
        {
            UpdateAgentClient();
            UpdateLogPath();
        }

        /// <summary>
        /// Updates the Agent client of the log box.
        /// </summary>
        private void UpdateAgentClient()
        {
            IAgentClient agentClient = adminContext.MainForm.GetAgentClient(ChildFormTag?.TreeNode);
            dataBox.AgentClient = agentClient;

            if (agentClient == null)
                dataBox.SetFirstLine(AdminPhrases.AgentNotEnabled);
            else
                dataBox.SetFirstLine(AdminPhrases.FileLoading);
        }

        /// <summary>
        /// Updates the path of the log box.
        /// </summary>
        private void UpdateLogPath()
        {
            dataBox.LogPath = new RelativePath(TopFolder.Comm, AppFolder.Log,
                CommUtils.GetDeviceLogFileName(deviceConfig.DeviceNum, ".txt"));
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void Save()
        {
            // do nothing
        }


        private void FrmDeviceData_Load(object sender, EventArgs e)
        {
            FormTranslator.Translate(this, GetType().FullName);
            Text = string.Format(Text, deviceConfig.DeviceNum);

            ChildFormTag.MainFormMessage += ChildFormTag_MainFormMessage;

            InitLogBox();
            tmrRefresh.Interval = ScadaUiUtils.LogRemoteRefreshInterval;
            tmrRefresh.Start();
        }

        private void FrmDeviceData_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrRefresh.Stop();
        }

        private void FrmDeviceData_VisibleChanged(object sender, EventArgs e)
        {
            tmrRefresh.Interval = Visible
                ? ScadaUiUtils.LogRemoteRefreshInterval
                : ScadaUiUtils.LogInactiveRefreshInterval;
        }

        private void ChildFormTag_MainFormMessage(object sender, FormMessageEventArgs e)
        {
            if (e.Message == AdminMessage.UpdateAgentClient)
                UpdateAgentClient();
        }

        private async void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                tmrRefresh.Stop();
                await Task.Run(() => dataBox.RefreshWithAgent());
                tmrRefresh.Start();
            }
        }

        private void btnDeviceProps_Click(object sender, EventArgs e)
        {
            // show the device properties if possible
            /*if (environment.TryGetKPView(kp, false, commLine.CustomParams, out KPView kpView, out string errMsg))
            {
                if (kpView.CanShowProps)
                {
                    kpView.ShowProps();

                    if (kpView.KPProps.Modified)
                    {
                        kp.CmdLine = kpView.KPProps.CmdLine;
                        ChildFormTag.SendMessage(this, CommMessage.UpdateLineParams);
                    }
                }
                else
                {
                    ScadaUiUtils.ShowWarning(CommShellPhrases.NoDeviceProps);
                }
            }
            else
            {
                ScadaUiUtils.ShowError(errMsg);
            }*/
        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            // show the device command form
            /*if (frmDeviceCommand == null)
                frmDeviceCommand = new FrmDeviceCommand(kp, environment);

            frmDeviceCommand.ShowDialog();*/
        }
    }
}