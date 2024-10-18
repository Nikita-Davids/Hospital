namespace Hospital.ModelViews
{
    public class PrescriptionFilterViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PrescriptionViewModel> Prescriptions { get; set; }
        public List<MedicineSummaryViewModel> MedicineSummary { get; set; }
    }
}