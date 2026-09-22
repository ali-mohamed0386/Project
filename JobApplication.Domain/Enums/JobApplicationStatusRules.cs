namespace JobApplication.Domain.Enums
{
    /// <summary>
    /// The rules governing allowed transitions between application statuses.
    /// This is the lifecycle of the entity itself, so it is pure domain logic
    /// and belongs here.
    /// </summary>
    /// <remarks>
    /// The forward-only state machine:
    /// <code>
    /// Applied ──▶ UnderReview ──▶ InterView ──▶ Accepted
    ///                                        └─▶ Rejected
    /// </code>
    /// Forbidden: moving backwards, skipping a stage, or changing a terminal state.
    /// </remarks>
    public static class JobApplicationStatusRules
    {
        private static readonly Dictionary<JobApplicationStatus, JobApplicationStatus[]> AllowedTransitions =
            new()
            {
                [JobApplicationStatus.Applied] = [JobApplicationStatus.UnderReview],
                [JobApplicationStatus.UnderReview] = [JobApplicationStatus.InterView],
                [JobApplicationStatus.InterView] = [JobApplicationStatus.Accepted, JobApplicationStatus.Rejected],
                [JobApplicationStatus.Accepted] = [],
                [JobApplicationStatus.Rejected] = [],
                [JobApplicationStatus.Cancelled] = [],
            };

        /// <summary>
        /// Determines whether the transition from the current status to the
        /// requested status is allowed.
        /// </summary>
        public static bool CanTransitionTo(this JobApplicationStatus current, JobApplicationStatus target)
            => AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);

        /// <summary>
        /// Cancellation is permitted only while the application is in the
        /// Applied or UnderReview state. It is forbidden once the application
        /// reaches the interview stage.
        /// </summary>
        public static bool CanBeCancelled(this JobApplicationStatus current)
            => current is JobApplicationStatus.Applied or JobApplicationStatus.UnderReview;
    }
}
