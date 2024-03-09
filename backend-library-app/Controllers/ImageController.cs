using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharpCifs.Smb;

namespace backend_library_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        //172.17.0.2
        private readonly string _sambaUrl = "smb://samba-container:1445/mount";
        private readonly string _username = "admin";
        private readonly string _password = "password1";

        // GET: api/Image/{fileName}
        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            try
            {
                // Lógica para obtener la imagen del servidor Samba
                byte[] imageData = GetFile(fileName);
                if (imageData != null)
                {
                    return File(imageData, "image/jpeg"); // Cambiar el tipo MIME según el tipo de imagen
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la imagen: {ex.Message}");
            }
        }

        // POST: api/Image
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Archivo no proporcionado");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();
                    string fileName = file.FileName;

                    UploadFile(imageData, fileName);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al subir la imagen: {ex.Message}");
            }
        }

        // PUT: api/Image/{fileName}
        [HttpPut("{fileName}")]
        public async Task<IActionResult> UpdateImage(string fileName, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Archivo no proporcionado");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    UpdateFile(imageData, fileName);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la imagen: {ex.Message}");
            }
        }

        // DELETE: api/Image/{fileName}
        [HttpDelete("{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                DeleteFile(fileName);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la imagen: {ex.Message}");
            }
        }

        private byte[] GetFile(string fileName)
        {
            try
            {
                var creds = new NtlmPasswordAuthentication(_sambaUrl, _username, _password);
                var file = new SmbFile(_sambaUrl + "/" + fileName, creds);
                using (var stream = file.GetInputStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        int bytesRead;
                        byte[] buffer = new byte[8192]; // Tamaño del buffer, puedes ajustarlo según sea necesario
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el archivo '{fileName}' de Samba: {ex.Message}");
            }
        }

        private void UploadFile(byte[] fileData, string fileName)
        {
            try
            {
                var creds = new NtlmPasswordAuthentication(_sambaUrl, _username, _password);
                var file = new SmbFile(_sambaUrl + "/" + fileName, creds);
                using (var stream = file.GetOutputStream())
                {
                    stream.Write(fileData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al subir el archivo '{fileName}' a Samba: {ex.Message}");
            }
        }

        private void UpdateFile(byte[] fileData, string fileName)
        {
            try
            {
                var creds = new NtlmPasswordAuthentication(_sambaUrl, _username, _password);
                var file = new SmbFile(_sambaUrl + "/" + fileName, creds);
                if (file.Exists())
                {
                    file.Delete();
                }
                using (var stream = file.GetOutputStream())
                {
                    stream.Write(fileData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el archivo '{fileName}' en Samba: {ex.Message}");
            }
        }

        private void DeleteFile(string fileName)
        {
            try
            {
                var creds = new NtlmPasswordAuthentication(_sambaUrl, _username, _password);
                var file = new SmbFile(_sambaUrl + "/" + fileName, creds);
                if (file.Exists())
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el archivo '{fileName}' de Samba: {ex.Message}");
            }
        }
    }
}
