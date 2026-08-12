using System;
using System.Collections.Generic;
using System.Text;

namespace Shabakat.Domain.Common
{
    public abstract class Base
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
