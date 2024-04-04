using Dapper;

using CielaDocs.DocStoreService.DataAccess;
using CielaDocs.DocStoreService.Models;


using Microsoft.IO;



using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.DocStoreService.Repository
{
    public class DocumentRepository : IDocumentRepository
    {
        
        private readonly DapperContext _context;

        public DocumentRepository(DapperContext context)
        {
            this._context = context;
        }
        public async Task<bool> CheckDocumentExists(long Id)
        {
            var query = "Select top 1 Id from document where Id=@Id";

            using (var connection = this._context.CreateConnection())
            {
                var docs = await connection.QuerySingleOrDefaultAsync<long?>(query, new { Id });
                return (docs != null);
            }
        }

        public async Task<long> GetDocumentIDbyTmpGuid(string tmpGuid)
        {
            var query = "Select top 1 Id from document where tmpGuid=@tmpGuid";

            using (var connection = this._context.CreateConnection())
            {
                var docs = await connection.QuerySingleOrDefaultAsync<long?>(query, new { tmpGuid });
                return docs ?? 0;
            }
        }

        public async Task<IEnumerable<long?>> GetDocumentsByTmpGuid(string tmpGuid)
        {
            var query = "Select Id from document where tmpGuid=@tmpGuid";

            using (var connection = this._context.CreateConnection())
            {
                var docs = await connection.QueryAsync<long?>(query, new { tmpGuid });
                return docs ?? new List<long?>();
            }
        }



        public async Task<long> SaveDocummentAsync(byte[] doc, string fileName = "", int createdBy = 0, long parentID = 0, long fkId = 0, int groupTableId = 0, string tmpGuid = "", int documentTypeId = 0)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            await using SqlCommand cmd1 = connection.CreateCommand();
            cmd1.CommandText = $@"INSERT INTO [dbo].[Document]([DocType],[DocName],[Description],[CreatedBy],[LastModifiedBy],[ParentID],[FkID],[GroupTableID],[TmpGuid],[DocumentTypeID]) 
                                        OUTPUT
                                              inserted.Id
                                        values (@DocType,@DocName,@Description,@CreatedBy,@LastModifiedBy,@ParentID,@FkID,@GroupTableID,@TmpGuid,@DocumentTypeID)";

            cmd1.Transaction = transaction;
            cmd1.Parameters.AddRange(
                new SqlParameter[]
                {
                    new SqlParameter("DocType", Path.GetExtension(fileName)),
                   new SqlParameter("DocName", Path.GetFileName(fileName)),
            new SqlParameter("Description", string.Empty),
            new SqlParameter("CreatedBy",createdBy),
            new SqlParameter("LastModifiedBy",createdBy),
            new SqlParameter("ParentID", parentID),
            new SqlParameter("FkID", fkId),
            new SqlParameter("GroupTableID", groupTableId),
            new SqlParameter("TmpGuid", tmpGuid),
            new SqlParameter("DocumentTypeID", documentTypeId)
          });

            await using SqlDataReader reader1 = await cmd1.ExecuteReaderAsync();
            if (!(await reader1.ReadAsync()) || (await reader1.IsDBNullAsync(0)))
            {
                throw new Exception("The inserted ID should have been returned.");
            }
            long newbId = reader1.GetInt64(0);
            await reader1.CloseAsync();



            await using SqlCommand cmd = connection.CreateCommand();

            cmd.CommandText =
                $@"INSERT INTO [dbo].[DocumentStore]
                           ([DocumentID]
                           ,[Document]
                           ,[DocGUID]
                           ,[DocType]
                           ,[DocName]
                           ,[Datum])
                OUTPUT
                   inserted.[Document].PathName(),
                    GET_FILESTREAM_TRANSACTION_CONTEXT()
                VALUES (
                    @DocumentID,
                    @Document,
                    NEWID(),
                    @DocType,
                    @DocName,
                    GETDATE())";
            cmd.Transaction = transaction;
            cmd.Parameters.AddRange(
                new SqlParameter[]
                {
                    new SqlParameter("DocumentID", newbId),
                    new SqlParameter("Document",doc),
                    new SqlParameter("DocType", Path.GetExtension(fileName)),
                   new SqlParameter("DocName", Path.GetFileName(fileName)),
                });

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (!(await reader.ReadAsync()) || (await reader.IsDBNullAsync(0)))
            {
                throw new Exception("The inserted ID should have been returned.");
            }

            string path = reader.GetString(0);
            byte[] transactionContext = reader.GetSqlBytes(1).Buffer
                ?? throw new Exception("TransactionContext should not be null.");
            await reader.CloseAsync();
            if (newbId > 0)
            {
                await transaction.CommitAsync();
                return newbId;
            }
            else
            {
                await transaction.RollbackAsync();
                return 0;
            }

        }
        public async Task<DocInfoDto?> GetDocInfoAsync(long documentId, CancellationToken ct) {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);

            string path = String.Empty;
            byte[] documentContent;
            string fileName = string.Empty;
            long fileSize = 0;

            await using (SqlCommand objSqlCmd = new SqlCommand("FileGet", connection, transaction))
            {
                objSqlCmd.CommandType = CommandType.StoredProcedure;

                SqlParameter objSqlParam1 = new SqlParameter("@Id", SqlDbType.BigInt);
                objSqlParam1.Value = documentId;

                objSqlCmd.Parameters.Add(objSqlParam1);

                await using (SqlDataReader sdr = await objSqlCmd.ExecuteReaderAsync(ct))
                {
                    if (!(await sdr.ReadAsync(ct)) ||
                    (await sdr.IsDBNullAsync(0, ct)))
                    {
                        throw new Exception("A Document with the provided documentId does't exist.");
                    }

                    path = sdr[0].ToString();
                    //fileType = sdr[1].ToString();
                    fileName = sdr[2].ToString();

                }
            }
            await using (SqlCommand objSqlCmd = new SqlCommand("SELECT GET_FILESTREAM_TRANSACTION_CONTEXT()", connection, transaction))
            {
                byte[] objContext = (byte[])objSqlCmd.ExecuteScalar();
                SqlFileStream objSqlFileStream = new SqlFileStream(path, objContext, FileAccess.Read);
                documentContent = new byte[(int)objSqlFileStream.Length];
                objSqlFileStream.Read(documentContent, 0, documentContent.Length);
                objSqlFileStream.Close();
            }
            transaction.Commit();
            return new DocInfoDto(documentContent, fileName, documentContent.Length);
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public async Task<DocInfoDto?> GetDocumentContentByIdAsync(long Id, CancellationToken ct)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            return await this.GetDocInfoAsync(Id, ct);
        }

        public async Task<IEnumerable<DocumentDto>> GetFilteredDocumentsAsync(long fkid, int tableid, CancellationToken ct)
        {
                    var query = @"select d.Id
               ,d.DocName
              ,d.Description
              ,d.LastModifiedOn
              ,d.LastModifiedBy
              ,d.ParentID
              ,d.FkID
              ,d.GroupTableID
              ,d.TmpGuid
              ,d.DocumentTypeID
              ,d.StatusID
              ,t.Name as DocumentTypeName
              ,s.Name as DocumentStatusName
              FROM Document d
              left join DocumentType t on d.DocumentTypeID=t.id
              left join DocumentStatus s on d.StatusID=s.id
              where d.fkid=@fkid and d.GroupTableID=@tableid";

            using (var connection = this._context.CreateConnection())
            {
                var dbResult =
                     await connection.QueryAsync<DocumentDto>(
                         new CommandDefinition(
                             query,
                             parameters: new { fkid,tableid },
                             cancellationToken: ct));

                return dbResult;
            }
        }

        public async Task<int> DeleteDocumentByIdAsync(long id, CancellationToken ct)
        {
            var query1 = @$"delete from document where id={id} delete from documentStore where DocumentID={id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync(ct);
            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);
            await using (SqlCommand objSqlCmd = new SqlCommand(query1, connection, transaction))
            {
                await objSqlCmd.ExecuteScalarAsync();
            }
            transaction.Commit();
            return 1;

        }
    }
}
