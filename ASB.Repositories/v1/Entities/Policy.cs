namespace ASB.Repositories.v1.Entities
{
    /// <summary>
    /// Represents a policy that can be applied to a resource. Policies define rules and configurations for resources such as queues, topics, and subscriptions. They can include settings for access control, message handling, and other operational parameters.
    /// </summary>
    public class Policy
    {
        public int Id { get; set; }
        /// <summary>
        /// The name of the policy.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// A description of the policy.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The resource this policy applies to (e.g., "Invoice", "User", "Report").
        /// </summary>
        public required string Resource { get; set; }

        /// <summary>
        /// The action permitted on the resource (e.g., "Read", "Write", "Delete").
        /// </summary>
        public required string Action { get; set; }
    }
}