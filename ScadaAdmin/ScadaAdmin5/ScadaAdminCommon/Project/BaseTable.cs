﻿/*
 * Copyright 2018 Mikhail Shiryaev
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
 * Module   : ScadaAdminCommon
 * Summary  : Represents the table of the configuration database
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2018
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Scada.Admin.Project
{
    /// <summary>
    /// Represents the table of the configuration database.
    /// <para>Представляет таблицу базы конфигурации.</para>
    /// </summary>
    public class BaseTable<T> : IBaseTable
    {
        /// <summary>
        /// The primary key of the table.
        /// </summary>
        protected string primaryKey;
        /// <summary>
        /// The property that is a primary key.
        /// </summary>
        protected PropertyDescriptor primaryKeyProp;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public BaseTable(string name, string primaryKey, string title)
        {
            Name = name;
            PrimaryKey = primaryKey;
            Title = title;
            Items = new SortedDictionary<int, T>();
        }


        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the primary key of the table.
        /// </summary>
        public string PrimaryKey
        {
            get
            {
                return primaryKey;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("The primary key can not be empty.");

                PropertyDescriptor prop = TypeDescriptor.GetProperties(ItemType)[value];

                if (prop == null)
                    throw new ArgumentException("The primary key property not found.");

                if (prop.PropertyType != typeof(int))
                    throw new ArgumentException("The primary key must be an integer.");

                primaryKey = value;
                primaryKeyProp = prop;
            }
        }

        /// <summary>
        /// Gets or sets the table title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the type of the table items.
        /// </summary>
        public Type ItemType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Gets the table items sorted by primary key.
        /// </summary>
        public SortedDictionary<int, T> Items { get; protected set; }


        /// <summary>
        /// Gets the primary key value of the item.
        /// </summary>
        protected int GetPrimaryKey(T item)
        {
            return (int)primaryKeyProp.GetValue(item);
        }

        /// <summary>
        /// Gets the table file name.
        /// </summary>
        protected string GetFileName(string directory)
        {
            return Path.Combine(directory, Name + ".xml");
        }


        /// <summary>
        /// Adds or updates an item in the table.
        /// </summary>
        public void AddItem(T item)
        {
            Items[GetPrimaryKey(item)] = item;
        }

        /// <summary>
        /// Adds or updates an item in the table.
        /// </summary>
        public void AddObject(object obj)
        {
            if (obj is T item)
                AddItem(item);
        }

        /// <summary>
        /// Loads the table from the specified file.
        /// </summary>
        public void Load(string fileName)
        {
            Items.Clear();
            List<T> list;
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                list = (List<T>)serializer.Deserialize(reader);
            }

            foreach (T item in list)
            {
                Items.Add(GetPrimaryKey(item), item);
            }
        }

        /// <summary>
        /// Tries to load the table from the specified file.
        /// </summary>
        public bool Load(string directory, out string errMsg)
        {
            try
            {
                string fileName = GetFileName(directory);

                if (File.Exists(fileName))
                    Load(fileName);

                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = AdminPhrases.LoadBaseTableError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Saves the table to the specified file.
        /// </summary>
        public void Save(string fileName)
        {
            List<T> list = new List<T>(Items.Values);
            XmlSerializer serializer = new XmlSerializer(list.GetType());
            XmlWriterSettings writerSettings = new XmlWriterSettings() { Indent = true };

            using (XmlWriter writer = XmlWriter.Create(fileName, writerSettings))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Tries to save the table to the specified file.
        /// </summary>
        public bool Save(string directory, out string errMsg)
        {
            try
            {
                Directory.CreateDirectory(directory);
                Save(GetFileName(directory));
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = AdminPhrases.SaveBaseTableError + ": " + ex.Message;
                return false;
            }
        }
    }
}
