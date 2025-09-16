namespace visita_booking_api.Models.Enums
{
    /// <summary>
    /// Status of accommodation approval process
    /// </summary>
    public enum AccommodationStatus
    {
        /// <summary>
        /// Accommodation is submitted and pending review
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Accommodation is approved and active
        /// </summary>
        Approved = 1,
        
        /// <summary>
        /// Accommodation is rejected and needs corrections
        /// </summary>
        Rejected = 2,
        
        /// <summary>
        /// Accommodation is suspended by admin
        /// </summary>
        Suspended = 3
    }
}