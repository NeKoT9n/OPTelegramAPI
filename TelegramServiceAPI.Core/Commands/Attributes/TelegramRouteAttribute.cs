using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramServiceAPI.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TelegramRouteAttribute(params string[] patterns) : Attribute
    {
        public string[] Patterns { get; } = patterns;
    }
}
