using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    /// <summary>
    /// Номенклатура на населените места в страната
    /// </summary>
    public partial class Town
    {
        public Town()
        {

        }

        /// <summary>
        /// PK
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Префикс на населено място
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Наименование на населено място
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Указател към област
        /// </summary>
        public string Oblast { get; set; }
        /// <summary>
        /// Указател към община
        /// </summary>
        public string Obstina { get; set; }
        /// <summary>
        /// Указател към кметство
        /// </summary>
        public string Kmetstvo { get; set; }
        /// <summary>
        /// Идентификатор на населено място
        /// </summary>
        public int? TownId { get; set; }
        /// <summary>
        /// Указател към община (Municipality-&gt;ID)
        /// </summary>
        public int? MunId { get; set; }
        public string NasmeName=>Prefix+Name;
     

    }
}
