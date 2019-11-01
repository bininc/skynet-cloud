﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace UWay.Skynet.Cloud.Mvc
{
    /// <summary>
    /// Claims extentions
    /// </summary>
    public static class ClaimsPrincipalExtentions
    {
        const string USER_NAME = "user_name";
        const string USER_ID = "id";
        const string DEPT_ID = "org";
        const string RESOURCES = "authorities";

        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <param name="principal">参数</param>
        /// <returns></returns>
        public static string  UserName(this ClaimsPrincipal principal)
        {
            if(principal.Identity.Name.IsNullOrEmpty())
             return principal.Claims.Single(p => p.Type.Equals(USER_NAME, StringComparison.InvariantCultureIgnoreCase)).Value;
            return principal.Identity.Name;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="principal">参数</param>
        /// <returns></returns>
        public static long? UserId(this ClaimsPrincipal principal)
        {
            return principal.Claims.Single(p => p.Type.Equals(USER_ID, StringComparison.InvariantCultureIgnoreCase)).Value.ToNullLong();
        }

        /// <summary>
        /// 获取部门ID
        /// </summary>
        /// <param name="principal">参数</param>
        /// <returns></returns>
        public static long? DeptId(this ClaimsPrincipal principal)
        {
            return principal.Claims.Single(p => p.Type.Equals(DEPT_ID, StringComparison.InvariantCultureIgnoreCase)).Value.ToNullLong();
        }

        public static bool HasPermission(this ClaimsPrincipal principal,string persmission)
        {
            var resource = principal.Claims.Where(p => p.Type.Equals(RESOURCES));
            return resource.Any(o => o.Value.Equals(persmission, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
