using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номеклатура на общините в страната
    /// </summary>
    public partial class Municipality
    {
        /// <summary>
        /// PK
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// указател към Town-ID - населено място
        /// </summary>
        public string Ekatte { get; set; }
        /// <summary>
        /// Община - наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Идентификатор на община
        /// </summary>
        public int? MunId { get; set; }
        /// <summary>
        /// Идентификатор на област
        /// </summary>
        public int? RegId { get; set; }

    }
}
