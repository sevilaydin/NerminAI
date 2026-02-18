using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerminAI.Domain.Entities
{
    public class Chunk:BaseEntity
    {
        public Guid DocumentId { get; set; }
        public string Content { get; set; }=string.Empty;
        public int ChunkIndex { get; set; }
        public int TokenCount { get; set; }

        public Document Document { get; set; }
        public Embedding? Embedding { get; set; }
    }
}
