namespace Accessioning.Core.Dtos.Patient
{
    using Accessioning.Core.Dtos.Shared;

    public class PatientParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}