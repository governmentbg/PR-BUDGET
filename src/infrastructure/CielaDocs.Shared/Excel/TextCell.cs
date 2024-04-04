using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CreateExcelFile
{
    public class TextCell : Cell
    {
        public TextCell(string header, string text, int index)
        {
            
            this.DataType = CellValues.InlineString;
            this.CellReference = header + index;
            //Add text to the text cell.
            this.InlineString = new InlineString { Text = new Text { Text = text } };
        }

 
    }
}
