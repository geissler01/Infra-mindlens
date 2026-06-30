using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Filters;

public class TreatmentFilters : PaginationFilters
{
    public Guid? PatientId { get; set; }
    
    [EnumDataType(typeof(TreatmentSessionDay))]
    public TreatmentSessionDay? SessionDay { get; set; }
    
    [EnumDataType(typeof(TreatmentState))]
    public TreatmentState? State { get; set; }
}