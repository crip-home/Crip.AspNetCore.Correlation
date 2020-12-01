namespace Crip.AspNetCore.Correlation
{
    /// <summary>
    /// Correlation identifier options.
    /// </summary>
    public class CorrelationIdOptions
    {
        /// <summary>
        /// The default correlation key value.
        /// </summary>
        public const string CorrelationPropertyName = "CorrelationId";

        /// <summary>
        /// Gets or sets the logging field name where the correlation identifier will be
        /// written.
        /// </summary>
        public string PropertyName { get; set; } = CorrelationPropertyName;

        /// <summary>
        /// Gets or sets the header field name where the correlation identifier will be
        /// sent/received from.
        /// </summary>
        public string Header { get; set; } = CorrelationPropertyName;

        /// <summary>
        /// Gets or sets a value indicating whether correlation identifier should be returned
        /// in the response headers.
        /// </summary>
        public bool IncludeInResponse { get; set; } = true;
    }
}