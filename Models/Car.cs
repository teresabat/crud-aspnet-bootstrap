using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace crud_dotnet.Models
{
    public class Car
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string email {get; set; }
    }
}