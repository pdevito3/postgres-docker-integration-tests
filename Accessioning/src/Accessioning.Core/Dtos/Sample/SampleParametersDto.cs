namespace Accessioning.Core.Dtos.Sample
{
    using Accessioning.Core.Dtos.Shared;

    public class SampleParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}