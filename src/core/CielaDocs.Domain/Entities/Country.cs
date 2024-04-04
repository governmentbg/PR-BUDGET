using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номенклатура на държавите
    /// </summary>
    public partial class Country
    {
    
        /// <summary>
        /// PK
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Наименование на държава
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ISO код на държавата
        /// </summary>
        public string Code { get; set; }

    
    }
}
