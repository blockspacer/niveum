﻿using System;
using System.Collections.Generic;
using System.Linq;
using Database.Database;

namespace Database.Database
{
    public partial interface IDataAccess
    {
        List<String> SelectManyMailAttachmentForNameById(int Id);
    }
}
