using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CreateExcelFile
{
    public class DateCell : Cell
    {
        public DateCell(string header, DateTime dateTime, int index)
        {

            this.DataType = CellValues.Number;
            this.CellReference = header + index;
            this.StyleIndex = 1;
            this.CellValue = new CellValue { Text = dateTime.ToOADate().ToString() }; 
            
        }
      
    }
}
