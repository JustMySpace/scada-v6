﻿/*
 * Copyright 2021 Rapid Software LLC
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
 * Module   : ScadaWebCommon
 * Summary  : Represents a record containing current data of an input channel
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Scada.Data.Models;

namespace Scada.Web.Api
{
    /// <summary>
    /// Represents a record containing current data of an input channel.
    /// <para>Представляет запись, содержащую текущие данные входного канала.</para>
    /// </summary>
    public struct CurDataRecord
    {
        /// <summary>
        /// Gets or sets the numeric input channel data.
        /// </summary>
        public CurDataPoint Pt { get; set; }

        /// <summary>
        /// Gets or sets the formatted input channel data.
        /// </summary>
        public CnlDataFormatted Fd { get; set; }
    }
}