using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Filters;

public class JournalingAnswerFilters : PaginationFilters
{
    [EnumDataType(typeof(JournalingEntryType))]
    public JournalingEntryType? EntryType { get; set; }
    
    [EnumDataType(typeof(JournalingState))]
    public JournalingState? State { get; set; }
}