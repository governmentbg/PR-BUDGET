using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номенклатура на областите в страната
    /// </summary>
    public partial class Region
    {
        /// <summary>
        /// PK
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// указател към населено място (Town-&gt;Id)
        /// </summary>
        public string Ekatte { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// идентификатор на област
        /// </summary>
        public int? RegId { get; set; }
    }
}
