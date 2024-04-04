using DevExpress.Spreadsheet;
using DevExpress.AspNetCore.RichEdit;

using System;
using System.IO;

public class SpreadsheetDocumentContentFromBytes
{
    public string DocumentId { get; set; }
    public Func<byte[]> ContentAccessorByBytes { get; set; }
    public DevExpress.Spreadsheet.DocumentFormat DocumentFormat { get; set; }

    public SpreadsheetDocumentContentFromBytes(string documentId, Func<byte[]> contentAccessorByBytes)
    {
        DocumentId = documentId;
        ContentAccessorByBytes = contentAccessorByBytes;
    }
    public SpreadsheetDocumentContentFromBytes(string documentId, DevExpress.Spreadsheet.DocumentFormat documentFormat, Func<byte[]> contentAccessorByBytes) : this(documentId, contentAccessorByBytes)
    {
        DocumentFormat = documentFormat;
    }
}

public class SpreadsheetDocumentContentFromStream
{
    public string DocumentId { get; set; }
    public Func<Stream> ContentAccessorByStream { get; set; }
    public DevExpress.Spreadsheet.DocumentFormat DocumentFormat { get; set; }

    public SpreadsheetDocumentContentFromStream(string documentId, Func<Stream> contentAccessorByStream)
    {
        DocumentId = documentId;
        ContentAccessorByStream = contentAccessorByStream;
    }
    public SpreadsheetDocumentContentFromStream(string documentId, DevExpress.Spreadsheet.DocumentFormat documentFormat, Func<Stream> contentAccessorByStream) : this(documentId, contentAccessorByStream)
    {
        DocumentFormat = documentFormat;
    }
}
