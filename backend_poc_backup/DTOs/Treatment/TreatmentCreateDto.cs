using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.Treatment;

public class TreatmentCreateDto
{
    [Required]
    public Guid PatientId { get; set; }
    
    [Required]
    [EnumDataType(typeof(TreatmentSessionDay))]
    public TreatmentSessionDay SessionDay { get; set; }
}