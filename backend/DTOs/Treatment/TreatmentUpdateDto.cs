using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.Treatment;

public class TreatmentUpdateDto
{
    [EnumDataType(typeof(TreatmentSessionDay))]
    public TreatmentSessionDay? SessionDay { get; set; }
    
    [EnumDataType(typeof(TreatmentState))]
    public TreatmentState? State { get; set; }
}