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
 * Module   : ScadaCommon
 * Summary  : Represents the base class for storage logic
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Scada.Data.Tables;
using System;
using System.Xml;

namespace Scada.Storages
{
    /// <summary>
    /// Represents the base class for storage logic.
    /// <para>Представляет базовый класс логики хранилища.</para>
    /// </summary>
    public abstract class StorageLogic : IStorage
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public StorageLogic(StorageContext storageContext)
        {
            StorageContext = storageContext ?? throw new ArgumentNullException(nameof(storageContext));
            IsReady = false;
            ViewAvailable = false;
        }


        /// <summary>
        /// Gets the storage context.
        /// </summary>
        protected StorageContext StorageContext { get; }

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public ServiceApp App => StorageContext.App;

        /// <summary>
        /// Gets or sets a value indicating whether the storage is ready for reading and writing.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Gets a value indicating whether a client application can load a view from the storage.
        /// </summary>
        public bool ViewAvailable { get; protected set; }


        /// <summary>
        /// Loads the configuration from the XML node.
        /// </summary>
        public virtual void LoadConfig(XmlElement xmlElement)
        {
            if (xmlElement == null)
                throw new ArgumentNullException(nameof(xmlElement));

            ViewAvailable = xmlElement.GetChildAsBool("ViewAvailable");
        }

        /// <summary>
        /// Makes the storage ready for operating.
        /// </summary>
        public virtual void MakeReady()
        {
        }

        /// <summary>
        /// Closes the storage.
        /// </summary>
        public virtual void Close()
        {
        }

        /// <summary>
        /// Reads text from the file.
        /// </summary>
        public abstract string ReadText(DataCategory category, string path);

        /// <summary>
        /// Reads the table of the configuration database.
        /// </summary>
        public abstract void ReadBaseTable(IBaseTable baseTable);

        /// <summary>
        /// Writes text to the file.
        /// </summary>
        public abstract void WriteText(DataCategory category, string path, string content);
    }
}