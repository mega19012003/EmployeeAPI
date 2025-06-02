using System;

namespace EmployeeAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerGroupOrderAttribute : Attribute
    {
        public int Order { get; }

        public SwaggerGroupOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
