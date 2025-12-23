namespace BeerCompetition.Competition.Domain.Entities
{
    /// <summary>
    /// Status of a tenant account.
    /// </summary>
    public enum TenantStatus
    {
        /// <summary>
        /// Account is active and can create/access competitions.
        /// </summary>
        Active,

        /// <summary>
        /// Account is suspended (e.g., payment issues, policy violation).
        /// Cannot create new competitions or access existing data.
        /// </summary>
        Suspended,

        /// <summary>
        /// Account is marked as deleted (soft delete).
        /// Data is retained for audit purposes but account is inactive.
        /// </summary>
        Deleted
    }
}
