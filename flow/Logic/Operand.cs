using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Mchnry.Flow.Logic
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Operand
    {
        [EnumMember(Value = "Or")]
        Or = 0,
        [EnumMember(Value = "And")]
        And = 1
    }
}
