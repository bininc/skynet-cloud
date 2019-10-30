﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWay.Skynet.Cloud.Ccs.Entity;
namespace UWay.Skynet.Cloud.Ccs.Service.Interface
{
    public interface IDictItemService
    {
        List<DictItem> GetDictItems(string dictType);

        List<DictItem> GetDictItems(string dictType,string dictCode);
    }
}