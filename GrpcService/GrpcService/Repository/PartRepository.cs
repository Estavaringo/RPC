using System.Collections.Generic;

namespace GrpcService.Repository
{
    public class PartRepository 
    {
        public PartRepository()
        {
            Parts = new List<Part>();
        }

        public List<Part> Parts { get; set; }
    }
}