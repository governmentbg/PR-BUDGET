using CielaDocs.DocStoreService.Models;
using CielaDocs.DocStoreService.Repository;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using gRpcFileTransfer;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static gRpcFileTransfer.FileService;
using static gRpcFileTransfer.GetDocumentsbyTmpGuidResponse.Types;

namespace CielaDocs.DocStoreService.Services
{
    public class FileUploadService : FileServiceBase
  {
        private readonly ILogger _logger;
        readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentRepository _docrepo;

        public FileUploadService(ILoggerFactory loggerFactory, IWebHostEnvironment webHostEnvironment, IDocumentRepository docrepo)
        {
            this._logger = loggerFactory.CreateLogger<FileUploadService>();
           this. _webHostEnvironment = webHostEnvironment;
            this._docrepo = docrepo;
        }
  public async override Task<FileUploadResponse> FileUpLoad(IAsyncStreamReader<FileUploadRequest> requestStream, ServerCallContext context)
  {
  
      int count = 0;
      var data = new List<byte>();

      RequestFileUpload req=null;

      while (await requestStream.MoveNext())
      {
        if (count++ == 0)
        {
            req = new RequestFileUpload
            {
              FileName = requestStream.Current.Metadata.FileName ?? string.Empty,
              CreatedBy = requestStream.Current.Metadata.CreatedBy,
              ParentID = requestStream.Current.Metadata.ParentId,
              FkID = requestStream.Current.Metadata.FkId,
              GroupTableID = requestStream.Current.Metadata.GroupTableId,
              TmpGuid = requestStream.Current.Metadata.TmpGuid,
              DocumentTypeID = requestStream.Current.Metadata.DocumentTypeId

            };
         }
        data.AddRange(requestStream.Current.Data);
      }

      var z = await this._docrepo.SaveDocummentAsync(data.ToArray(), req.FileName, req?.CreatedBy??0,req?.ParentID??0,req?.FkID??0, req.GroupTableID,req.TmpGuid, req.DocumentTypeID);
      return new FileUploadResponse { DocumentId = z };

    }

        public override async Task FileDownLoad(FileDownloadRequest request, IServerStreamWriter<BytesContent> responseStream, ServerCallContext context)
        {
 
           CancellationToken cancellationToken = context.CancellationToken;
            var blob = await this._docrepo.GetDocumentContentByIdAsync(request?.DocumentId ?? 0, cancellationToken);
            this._logger.LogInformation($"fileName={blob.Filename} fileContentL Length={blob.DocumentContent.Length} fileSize={blob.Size}");
            BytesContent content = new BytesContent
            {

                FileSize = blob.Size,
                Info = new gRpcFileTransfer.FileInfo { FileName = blob.Filename, FileExtension = Path.GetExtension(blob.Filename) },
                ReadedByte = 0
            };
            using MemoryStream memStream = new MemoryStream(blob.DocumentContent);
            byte[] buffer = new byte[2048];
            
            while ((content.ReadedByte = memStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                content.Buffer = ByteString.CopyFrom(buffer);
                await responseStream.WriteAsync(content);
            }

            memStream.Close();
        }
        public override async Task<CheckDocumentResponse> CheckDocumentExists(CheckDocumentRequest request, ServerCallContext context)=> new CheckDocumentResponse { DocExists = await this._docrepo.CheckDocumentExists(request.Id) };
        public override async Task<GetDocumentIDbyTmpGuidResponse> GetDocumentIDbyTmpGuid(GetDocumentIDbyTmpGuidRequest request, ServerCallContext context) => new GetDocumentIDbyTmpGuidResponse { Id = await this._docrepo.GetDocumentIDbyTmpGuid(request.Tmpguid) };
        public override async Task<GetDocumentsbyTmpGuidResponse> GetDocumentsbyTmpGuid(GetDocumentsbyTmpGuidRequest request, ServerCallContext context)  {
            var res = new GetDocumentsbyTmpGuidResponse();
            List<Idarray> lst = new List<Idarray>();
            var result = (await this._docrepo.GetDocumentsByTmpGuid(request.Tmpguid)).ToList();
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    lst.Add(new Idarray { Id = item??0 });
                }
            }
            res.Ids.AddRange(lst);
            return res;
        }
        public static Document DocumentMapper(DocumentDto doc)
        {
            var d = new Document();
            d.Id = doc.Id;
            d.DocName = doc.DocName;
            d.Description = doc.Description;
            d.LastModifiedOn = Timestamp.FromDateTimeOffset((DateTime)doc.LastModifiedOn);
            d.LastModifiedBy = doc.LastModifiedBy;
            d.ParentId = doc.ParentID??0;
            d.FkId = doc.FkID ?? 0;
            d.GroupTableId = doc.GroupTableID ?? 0;
            d.TmpGuid = doc.TmpGuid;
            d.DocumentTypeId=doc.DocumentTypeID?? 0;
            d.StatusId = doc.StatusID?? 0;
            d.DocumentTypeName = doc.DocumentTypeName;
            d.DocumentStatusName= doc.DocumentStatusName;

            return d;
        }
        public override async Task<FilteredDocumentsResponse> GetFilteredDocuments(FilteredDocumentsRequest request, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            var res= new FilteredDocumentsResponse();
            var ret = await this._docrepo.GetFilteredDocumentsAsync(request.FkId, request.TableId,cancellationToken);
            List<Document> lp = ret.ToList().ConvertAll(new Converter<DocumentDto, Document>(DocumentMapper));
            res.Result.AddRange(lp);
            return res;
        
        }
        public override async Task<DeleteDocumentByIdResponse> DeleteDocumentById(DeleteDocumentByIdRequest request, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            var res = new DeleteDocumentByIdResponse();
            var ret = await this._docrepo.DeleteDocumentByIdAsync(request.Id, cancellationToken);
            return res;
        }
    }
}
