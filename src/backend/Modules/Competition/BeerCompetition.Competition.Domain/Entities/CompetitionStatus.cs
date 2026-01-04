namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Represents the status of a competition in its lifecycle.
/// Follows state machine pattern for competition workflow.
/// </summary>
public enum CompetitionStatus
{
    /// <summary>
    /// Competition is being created and configured.
    /// Not visible to entrants yet.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Competition is open for entry registration.
    /// Entrants can submit entries and make payments.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Registration is closed, judging is in progress.
    /// No new entries can be submitted.
    /// </summary>
    Judging = 2,

    /// <summary>
    /// Judging is complete, results are being reviewed.
    /// Preparing for results publication.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Results have been published to entrants.
    /// Competition is archived.
    /// </summary>
    ResultsPublished = 4,

    /// <summary>
    /// Competition was canceled before completion.
    /// </summary>
    Canceled = 5
}
