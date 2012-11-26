﻿using System;
using System.Collections.Generic;
using System.Linq;
using DB = Database;

namespace Database
{
    public class TestService
    {
        private DataAccessManager dam;
        public TestService(DataAccessManager dam)
        {
            this.dam = dam;
        }

        public void SaveData(int SessionIndex, int Value)
        {
            using (var da = dam.Create())
            {
                da.UpsertOneTestRecord(new DB.TestRecord { SessionIndex = SessionIndex, Value = Value });
                da.Complete();
            }
        }

        public int LoadData(int SessionIndex)
        {
            using (var da = dam.Create())
            {
                var v = da.SelectOptionalTestRecordBySessionIndex(SessionIndex);
                if (v.OnHasValue)
                {
                    return v.HasValue.Value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void SaveLockData(int Value)
        {
            using (var da = dam.Create())
            {
                da.UpsertOneTestLockRecord(new DB.TestLockRecord { Id = 1, Value = Value });
                da.Complete();
            }
        }

        public void AddLockData(int Value)
        {
            using (var da = dam.Create())
            {
                var ov = da.LockOptionalTestLockRecordById(1);
                TestLockRecord v;
                if (ov.OnHasValue)
                {
                    v = ov.HasValue;
                }
                else
                {
                    v = new DB.TestLockRecord { Id = 1, Value = 0 };
                }
                v.Value += Value;
                da.UpsertOneTestLockRecord(v);
                da.Complete();
            }
        }

        public int LoadLockData()
        {
            using (var da = dam.Create())
            {
                var ov = da.SelectOptionalTestLockRecordById(1);
                TestLockRecord v;
                if (ov.OnHasValue)
                {
                    v = ov.HasValue;
                    return v.Value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}