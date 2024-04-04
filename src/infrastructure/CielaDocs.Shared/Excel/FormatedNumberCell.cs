using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CreateExcelFile
{
    public class FormatedNumberCell : NumberCell
    {
        public FormatedNumberCell(string header, string text, int index) : base(header, text, index)
        {
            this.StyleIndex = 2;
        }

    }
}
