﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWay.Skynet.Cloud.Ccs.Entity;
using UWay.Skynet.Cloud.Data;
using UWay.Skynet.Cloud.Request;
using UWay.Skynet.Cloud.Linq;
using UWay.Skynet.Cloud.Ccs.Repository;
using UWay.Skynet.Cloud.Ccs.Service.Interface;

namespace UWay.Skynet.Cloud.Ccs.Services
{
    public class CategoryService : ICategoryService
    {
        public long Add(CategoryItem user)
        {
            using (var uow = UnitOfWork.Get(Unity.ContainerName))
            {
                return new CategoryRepository(uow).Add(user);
            }
        }

        public int DeleteByIds(long[] arrayIds)
        {
            using (var uow = UnitOfWork.Get(Unity.ContainerName))
            {
                 new CategoryRepository(uow).Delete(arrayIds);
            }
            return 1;
        }

        public IList<CategoryItem> GetByCategoryId(int categoryId)
        {
            using (var uow = UnitOfWork.Get(Unity.ContainerName))
            {
                return new CategoryRepository(uow).Query().Where(p => p.CategoryId.Equals(categoryId)).ToList();
            }
        }

        public CategoryItem GetById(long id)
        {
            using (var uow = UnitOfWork.Get(Unity.ContainerName))
            {
                return new CategoryRepository(uow).GetById(id);
            }
        }

        public int Update(CategoryItem user)
        {
            throw new NotImplementedException();
        }
    }
}