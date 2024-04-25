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

        public CustomRequestController(IOptions<ConnectionStrings> ConnectionStrings)
        {
            _ConnectionStrings = ConnectionStrings.Value;
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
                }
                return Ok("se registró correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
            }
        }
    }
}
