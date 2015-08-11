namespace Azure.Util
{
    /// <summary>
    /// Description of IPlugin.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets the plugin_name.
        /// </summary>
        /// <value>The plugin_name.</value>
        string plugin_name { get; }

        /// <summary>
        /// Gets the plugin_version.
        /// </summary>
        /// <value>The plugin_version.</value>
        string plugin_version { get; }

        /// <summary>
        /// Gets the plugin_author.
        /// </summary>
        /// <value>The plugin_author.</value>
        string plugin_author { get; }

        /// <summary>
        /// Message_voids this instance.
        /// </summary>
        void message_void();

        /// <summary>
        /// Content_voids this instance.
        /// </summary>
        void content_void();

        /// <summary>
        /// Packets_voids this instance.
        /// </summary>
        void packets_void();

        /// <summary>
        /// Habbo_voids this instance.
        /// </summary>
        void habbo_void();
    }
}