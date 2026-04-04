namespace Domain.Exceptions
{
    public class TechnicianNotAvailableException : DomainException
    {
        public int TechnicianId { get; }
        public DateTime RequestedDate { get; }

        public TechnicianNotAvailableException(int technicianId, DateTime requestedDate)
            : base($"Kỹ thuật viên #{technicianId} không rảnh vào ngày {requestedDate:dd/MM/yyyy}")
        {
            TechnicianId = technicianId;
            RequestedDate = requestedDate;
        }
    }
}