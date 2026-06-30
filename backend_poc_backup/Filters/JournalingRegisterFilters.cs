using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Filters;

public class JournalingRegisterFilters : PaginationFilters
{
    [EnumDataType(typeof(JournalingRegisterType))]
    public JournalingRegisterType? Type { get; set; }
}