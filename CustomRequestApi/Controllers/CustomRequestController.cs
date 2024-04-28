using CustomRequestApi.Blob;
using CustomRequestApi.DTOs;
using CustomRequestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace CustomRequestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomRequestController : ControllerBase
    {
        public readonly ConnectionStrings _ConnectionStrings;
        public readonly ConnectionStringsBlob _ConnectionStringsBlob;
        private IBlobStorageService _blobStorageService;

        public CustomRequestController(IOptions<ConnectionStrings> ConnectionStrings, IOptions<ConnectionStringsBlob> connectionStringsBlob, IBlobStorageService blobStorageService)
        {
            _ConnectionStrings = ConnectionStrings.Value;
            _ConnectionStringsBlob = connectionStringsBlob.Value;
            _blobStorageService = blobStorageService;
        }

        [HttpPost]
        [Route("SaveCustomRequest")]
        public IActionResult SaveCustomRequest(CustomRequest customRequest)
        {
            if (customRequest.Name == null || customRequest.Description == string.Empty)
            {
                return BadRequest("La descripcion es requerida");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
                {
                    connection.Open();

                    string query = string.Format("INSERT INTO [dbo].[CustomRequest] (Id, Name, Description, Priority) " +
                                                 "VALUES (NEXT VALUE FOR SequenceCustomRequest, '{0}', '{1}', '{2}'); ",
                                                 customRequest.Name, customRequest.Description, customRequest.Priority);

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string querySequence = "SELECT CURRENT_VALUE FROM sys.sequences WHERE name = 'SequenceCustomRequest';";

                    using (SqlCommand commandSequence = new SqlCommand(querySequence, connection))
                    {
                        customRequest.Id = Convert.ToInt32(commandSequence.ExecuteScalar());
                    }

                    if(customRequest.Attachments != null)
                    {
                        foreach (var attachUrl in SaveFilesBlob(customRequest.Id, customRequest.Attachments))
                        {
                            query = string.Format("INSERT INTO [dbo].[Attachment] (RequestId, Url) " +
                                                 "VALUES ({0},'{1}'); ",
                                                 customRequest.Id, attachUrl);

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                return Ok("se registró correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("GetCustomRequests")]
        public List<CustomRequest> GetCustomRequests()
        {
            List<CustomRequest> customRequests = new List<CustomRequest>();
            using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
            {
                connection.Open();

                string query = "SELECT * FROM CustomRequest";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomRequest customRequest = new CustomRequest
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"] is DBNull ? null : (string)reader["Name"],
                            Description = reader["Description"] is DBNull ? null : (string)reader["Description"],
                            Priority = reader["Priority"] is DBNull ? null : (string)reader["Priority"],
                        };
                        customRequests.Add(customRequest);
                    }
                }
            }
            return customRequests;
        }

        [HttpPost]
        [Route("GetAttachments")]
        public List<string> GetAttachments(int Id)
        {
            List<string> attachments = new List<string>();

            using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
            {
                connection.Open();

                string query = "SELECT * FROM Attachment WHERE RequestId = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            attachments.Add(reader["Url"] is DBNull ? string.Empty : (string)reader["Url"]);
                        }
                    }
                }
            }
            return attachments;
        }

        private List<string> SaveFilesBlob(int Id, List<string>? attachments)
        {
            List<string> result = new List<string>();

            string container = "custom-request-" + Id;

            var containerClient = _blobStorageService.GetContainer(container);

            foreach (var attach in attachments)
            {
                byte[] bytesAttach = Convert.FromBase64String(attach.Split("\n**")[1].Split(";base64,")[1]);

                result.Add(_blobStorageService.AddBlob(new MemoryStream(bytesAttach), attach.Split("\n**")[0], containerClient));
            }

            return result;
        }
    }
}
