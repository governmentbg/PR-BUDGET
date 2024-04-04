using AutoMapper;

using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public class CourtDto:IMapFor<Court>
    {
      
        public int Id { get; set; }

        public int Num { get; set; }
        public int CourtTypeId { get; set; }


        public string Name { get; set; }

        public bool IsActive { get; set; }
        public string CourtGuid { get; set; }
        public string KontoCode { get; set; }

       public virtual CourtTypeDto CourtTypeDtos { get; set; }
    }
}
