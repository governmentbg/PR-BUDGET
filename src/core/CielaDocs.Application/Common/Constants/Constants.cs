using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Constants
{
    public static class CommonConstants
    {
        public enum LogMessageType { 
            Common=0,
            Add=1,
            Edit=2,
            Delete=3
        }
        public const string LogMsgForbiddenAdd = "Нямате предоставени права да добавяте данни";
        public const string LogMsgForbiddenEdit = "Нямате предоставени права да редактирате данни";
        public const string LogMsgForbiddenDel = "Нямате предоставени права да изтривате данни";

    }
}
