namespace discussionspot.Models.Domain
{
    public abstract class BaseEntity
    {
        /// <summary>
        /// Common creation timestamp for all entities
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Common last update timestamp for all entities
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
