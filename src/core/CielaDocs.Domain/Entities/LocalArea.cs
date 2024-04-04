using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номеклатура на районите за градове с районно делене
    /// </summary>
    public partial class LocalArea
    {
        /// <summary>
        /// PK
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// КОД по ЕКАТТЕ за населено място
        /// </summary>
        public string TownId { get; set; }
    }
}
