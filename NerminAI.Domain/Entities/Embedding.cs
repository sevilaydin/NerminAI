using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerminAI.Domain.Entities
{
    public class Embedding:BaseEntity
    {
        public Guid ChunkId { get; set; }
        public Vector Vector { get; set; } = null;
        public string Model { get; set; } = "all-MiniLM-L6-v2";

        public Chunk Chunk { get; set; }
    }
}
