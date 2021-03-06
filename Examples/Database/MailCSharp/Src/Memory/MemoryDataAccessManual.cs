﻿using System;
using System.Collections.Generic;
using System.Linq;
using Database.Database;
using Boolean = System.Boolean;
using String = System.String;
using Type = System.Type;
using Int = System.Int32;
using Real = System.Double;
using Byte = System.Byte;

namespace Database.Memory
{
    public partial class MemoryDataAccess : IDataAccess
    {
        public List<String> FromMailAttachmentSelectManyForNameById(Int64 Id)
        {
            return MemoryDataManipulate.FromMailAttachmentSelectManyById(this.Tables, this.Indices, Id).Select(e => e.Name).ToList();
        }
    }
}
