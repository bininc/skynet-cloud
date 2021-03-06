namespace UWay.Skynet.Cloud.Linq.Impl.Internal
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    
    using Reflection;

    internal class CustomTypeDescriptorPropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
    {
        private static readonly MethodInfo PropertyMethod = typeof(CustomTypeDescriptorExtensions).GetMethod("Property");
        private readonly Type propertyType;

        /// <exception cref="ArgumentException"><paramref name="elementType"/> did not implement <see cref="ICustomTypeDescriptor"/>.</exception>
        public CustomTypeDescriptorPropertyAccessExpressionBuilder(Type elementType, Type memberType, string memberName)
            : base(elementType, memberName)
        {
            if (!elementType.IsCompatibleWith(typeof(ICustomTypeDescriptor)))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, "ElementType: {0} did not implement {1}", elementType, typeof(ICustomTypeDescriptor)),
                    "elementType");
            }

            this.propertyType = GetPropertyType(memberType);
        }

        private Type GetPropertyType(Type memberType)
        {
            var descriptorProviderPropertyType = this.GetPropertyTypeFromTypeDescriptorProvider();
            if (descriptorProviderPropertyType != null)
            {
                memberType = descriptorProviderPropertyType;
            }

            //Handle value types for null and DBNull.Value support converting them to Nullable<>
            if (memberType.IsValueType && !memberType.IsNullable())
            {
                return typeof(Nullable<>).MakeGenericType(memberType);
            }

            return memberType;
        }

        private Type GetPropertyTypeFromTypeDescriptorProvider()
        {
            var propertyDescriptor = TypeDescriptor.GetProperties(this.ItemType)[this.MemberName];
            if (propertyDescriptor != null)
            {
                return propertyDescriptor.PropertyType;
            }

            return null;
        }

        public Type PropertyType
        {
            get
            {
                return this.propertyType;
            }
        }

        public override Expression CreateMemberAccessExpression()
        {
            ConstantExpression propertyNameExpression = Expression.Constant(this.MemberName);

            MethodCallExpression propertyExpression =
                Expression.Call(
                    PropertyMethod.MakeGenericMethod(this.propertyType),
                    this.ParameterExpression,
                    propertyNameExpression);

            return propertyExpression;
        }
    }
}