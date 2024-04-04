using CielaDocs.Application.Common.Mappings;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class RegionDto : IMapFrom<Region>
    {
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
