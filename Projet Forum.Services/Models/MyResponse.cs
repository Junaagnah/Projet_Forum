using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Models
{
    public class MyResponse
    {
        public bool Succeeded { get; set; }
        public List<string> Messages { get; set; }
        public Object Result { get; set; }

        public MyResponse()
        {
            Messages = new List<string>();
        }
    }
}
