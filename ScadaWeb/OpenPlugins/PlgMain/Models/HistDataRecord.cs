﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Data.Models;

namespace Scada.Web.Plugins.PlgMain.Models
{
    /// <summary>
    /// Represents a record containing historical data of an input channel.
    /// <para>Представляет запись, содержащую исторические данные входного канала.</para>
    /// </summary>
    public struct HistDataRecord
    {
        /// <summary>
        /// Gets or sets the numeric input channel data.
        /// </summary>
        public CnlData D { get; set; }

        /// <summary>
        /// Gets or sets the formatted input channel data.
        /// </summary>
        public CnlDataFormatted Df { get; set; }
    }
}