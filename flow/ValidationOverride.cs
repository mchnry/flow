using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    [Serializable]
    public class ValidationOverride
    {


        /// <summary>
        /// Internal constructor for ValidationOverride
        /// </summary>
        /// <remarks><list type="bullet">
        /// <item>Instance can only be constructed by calling <see cref="Validation.CreateOverride(string, string)">Validation.CreateOverride</see></item>
        /// </list></remarks>
        /// <param name="key">References the unique id of the <see cref="Validation">Validation</see> in the instance of <see cref="IValidationContainer"/></param>
        /// <param name="comment">Optionally provided by the calling system when overriding.  Can be user comments captured in UI, or static provided by caller.</param>
        /// <param name="auditCode">Optional code provided by consuming system to reference some system specific logging entry where this overrride is recorded</param>
        internal ValidationOverride(string key, string comment, string auditCode)
        {
            this.Key = key;
            this.Comment = comment;
            this.AuditCode = auditCode;
        }

        /// <summary>
        /// References the unique identifier of the <see cref="Validation">Validation</see> that this overrides.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; internal set; }

        /// <summary>
        /// Optionally provided by the calling system when overriding.  Can be user comments captured in UI, or static provided by caller.
        /// </summary>
        [JsonProperty("c")]
        public string Comment { get; internal set; }

        /// <summary>
        /// Optional code provided by consuming system to reference some system specific logging entry where this overrride is recorded
        /// </summary>
        [JsonProperty("ac")]
        public string AuditCode { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "rd")]
        public bool Redeemed { get; internal set; }

    }
}
