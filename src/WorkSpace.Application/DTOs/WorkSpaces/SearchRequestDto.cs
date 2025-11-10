using System;
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class SearchRequestDto
    {
        public string? Ward { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
 
        public int? Capacity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Amenities { get; set; }
        public string? Keyword { get; set; }


        public bool HasDateTimeFilter() => StartTime.HasValue || EndTime.HasValue;
     
    }
}