using AutoMapper;

using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class MetricsDto:IMapFor<Metrics>
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Calculation { get; set; }
        public string GKey { get; set; }
        public int FunctionalSubAreaId { get; set; }
        public virtual FunctionalSubAreaDto FunctionalSubAreaDtos { get; set; }
        public void Mapping(Profile profile)
        {
           



        }
    }
}
