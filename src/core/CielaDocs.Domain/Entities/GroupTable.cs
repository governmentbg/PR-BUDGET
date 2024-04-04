using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номенклатурни дании
    /// </summary>
    public partial class GroupTable
    {
        /// <summary>
        /// PK
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Флаг за логически изтрит запис
        /// </summary>
        public bool? IsDeleted { get; set; }
        /// <summary>
        /// Не се използва в момента
        /// </summary>
        public string App { get; set; }
    }
}
