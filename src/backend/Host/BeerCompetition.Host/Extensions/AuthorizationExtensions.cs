namespace BeerCompetition.Host.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Default policy: Authenticated user with tenant_id claim
                options.AddPolicy("AuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("tenant_id");
                });

                // Organizer only
                options.AddPolicy("OrganizerOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("organizer");
                    policy.RequireClaim("tenant_id");
                });

                // Judge only
                options.AddPolicy("JudgeOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("judge");
                    policy.RequireClaim("tenant_id");
                });

                // Entrant only
                options.AddPolicy("EntrantOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("entrant");
                    policy.RequireClaim("tenant_id");
                });

                // Steward only
                options.AddPolicy("StewardOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("steward");
                    policy.RequireClaim("tenant_id");
                });

                // Judge or Organizer
                options.AddPolicy("JudgeOrOrganizer", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("judge", "organizer");
                    policy.RequireClaim("tenant_id");
                });
            });

            return services;
        }
    }
}
