using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFinder.Shared.DTOs.Common
{
    public  class SkillDto
    {
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
    }
}
