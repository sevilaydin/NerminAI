using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerminAI.Domain.Entities
{
    public class Document:BaseEntity
    {
        public string Title { get; set; }=string.Empty;
        public string Content { get; set; }=string.Empty;
        public DocumentType Type { get; set; }
        public string? Metadata { get; set; }

        public ICollection<Chunk> Chunks { get; set; } = new List<Chunk>();

    }
}
