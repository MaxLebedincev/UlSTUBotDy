using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandlerUlSTU.DTO.Timetable
{
    public class TwoWeeks
    {
        public Week? week { get; set; }
        [JsonIgnore]
        public List<int>? numberWeek { get; set; }
    }
}
