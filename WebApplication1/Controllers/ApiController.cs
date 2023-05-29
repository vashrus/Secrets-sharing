using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;
using WebApplication1.Model;
using WebApplication1.Model;
using WebApplication1.Services;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ApiController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IWebHostEnvironment env;

        public DatabaseModel Model { get; }

        public ApiController(DatabaseModel model, IUserService userService, IWebHostEnvironment env)
        {
            Model = model;
            this.userService = userService;
            this.env = env;
        }


        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <param name="userReqeust">Email and Password</param>
        /// <returns>JWT Token</returns>
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserReqeust userReqeust) 
        {
            var user = Model.Users.FirstOrDefault(x => x.Mail == userReqeust.Email && x.Password == userReqeust.Password);
            if(user == null) 
            { 
                return BadRequest(); 
            }    
            else
            {
                return Ok(userService.GenerateJwtToken(userId: user.UserId));
            }

        }

        /// <summary>
        /// Registration user
        /// </summary>
        /// <param name="userReqeust">Email and Password</param>
        /// <returns>JWT Token</returns>
        [AllowAnonymous]
        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] UserReqeust userReqeust)
        {
           if(Model.Users.FirstOrDefault(x=>x.Mail == userReqeust.Email) != null)
           {
                return BadRequest("Пользователь уже существует");
           }
           var user = new User()
           {
                Mail = userReqeust.Email,
                Password = userReqeust.Password,
                UserFio = "САНЯ"
                
           };

           Model.Add(user);
           await Model.SaveChangesAsync();
            
           return Ok(userService.GenerateJwtToken(userId: user.UserId));
        }

        /// <summary>
        /// Upload File
        /// </summary>
        /// <param name="uploadedFile">Selected file by user</param>
        /// <param name="mustRemove">File is must be removed after download</param>
        /// <returns>File name</returns>
        [Authorize]
        [HttpPost("File")]
        public async Task<IActionResult> UploadUserFile(IFormFile uploadedFile, bool mustRemove)
        {
            if (uploadedFile == null)
                return BadRequest("Не передан файл");

            byte[] file;
            string fileName;
            using (var stream = uploadedFile.OpenReadStream())
            {
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string route = Path.Combine(location, "files");
                fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N")}";
                string fileroute = Path.Combine(route, $"{fileName}.txt");

                if (!Directory.Exists(route))
                {
                    Directory.CreateDirectory(route);

                }
                using (var fileStream = new FileStream(fileroute, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = Model.Users.FirstOrDefault(x=>x.UserId == userId);
            user.Files.Add(new Model.File()
            {
                Name = fileName,
                UserId = userId,
                IsRemoved = mustRemove
            });

            await Model.SaveChangesAsync();
            return Ok(fileName);
        }

        /// <summary>
        /// Get File
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns>Result of downloading file</returns>
        [AllowAnonymous]
        [HttpGet("GetFile/{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var file = Model.Files.FirstOrDefault(x=>x.Name == fileName);
            if (file == null)
                return BadRequest("Файл не найден");
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string route = Path.Combine(location, "files");
            string fullPath = route + "/" + fileName + ".txt";

            if(System.IO.File.Exists(fullPath)) 
            {
                var bytes = System.IO.File.ReadAllBytes(fullPath);
                
                if(file.IsRemoved)
                    System.IO.File.Delete(fullPath);
                
                return new FileContentResult(bytes, "text/plain") { FileDownloadName = "text.txt" };
            }
            else
            {
                return BadRequest("Файл удален");
            }
        }
        /// <summary>
        /// Upload Text
        /// </summary>
        /// <param name="uploadedText">Printed text by user</param>
        /// <param name="mustRemove">Text is must be removed after download</param>
        /// <returns>Text name</returns>
        [Authorize]
        [HttpPost("UploadUserText")]
        public async Task<IActionResult> UploadUserText(string uploadedText, bool mustRemove)
        {
            if (uploadedText == null)
                return BadRequest("Текст не может быть пустым");

            byte[] file;
            string fileName =  $"{DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N")}";
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string route = Path.Combine(location, "text");
            string fileroute = Path.Combine(route, $"{fileName}.txt");

            if (!Directory.Exists(route))
            {
                Directory.CreateDirectory(route);

            }

            System.IO.File.WriteAllText(fileroute, uploadedText );

            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = Model.Users.FirstOrDefault(x => x.UserId == userId);
            user.Texts.Add(new Model.Text()
            {
                Content = fileName,
                IsRemoved = mustRemove
            });

            await Model.SaveChangesAsync();
            return Ok(fileName);
        }

        /// <summary>
        /// Get Text
        /// </summary>
        /// <param name="textName">text name</param>
        /// <returns>Result of getting text</returns>
        [AllowAnonymous]
        [HttpGet("GetText/{textName}")]
        public async Task<IActionResult> GetText(string textName)
        {
            var text = Model.Texts.FirstOrDefault(x => x.Content == textName);
            if (text == null)
                return BadRequest("Текст не найден");

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string route = Path.Combine(location, "text");
            string fullPath = route + "/" + textName + ".txt";
            if (System.IO.File.Exists(fullPath))
            {
                var bytes = System.IO.File.ReadAllBytes(fullPath);

                if (text.IsRemoved)
                    System.IO.File.Delete(fullPath);

                return File(bytes, "text/plain");

            }
            else
            {
                return BadRequest("Текст удален");
            }
        }

        /// <summary>
        /// Get user Files
        /// </summary>
        /// <returns>Files list</returns>
        [HttpGet("GetFiles")]
        public async Task<IActionResult> GetFiles()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            return Ok(Model.Files.ToList().Where(x => x.UserId == userId && !FileIsRemoved(x.Name, "files")).Select(x => x.Name).ToList());
            
        }
        /// <summary>
        /// Get user texts
        /// </summary>
        /// <returns>Texts list</returns>
        [HttpGet("GetText")]
        public async Task<IActionResult> GetText()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
           
            return Ok(Model.Texts.ToList().Where(x => x.UserId == userId && !FileIsRemoved(x.Content,"text")).Select(x => x.Content).ToList());
        }


        private bool FileIsRemoved(string fileName, string folder)
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string route = Path.Combine(location, folder);
            string fullPath = route + "/" + fileName + ".txt";
            return !System.IO.File.Exists(fullPath);
        }

        /// <summary>
        /// Delete a user text
        /// </summary>
        /// <param name="textName">text Name</param>
        /// <returns>Deletion result</returns>
        [HttpPost("DeleteText")]
        public async Task<IActionResult> DeleteText(string textName)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = Model.Users.FirstOrDefault(x => x.UserId == userId);
            var text = Model.Texts.FirstOrDefault(x=>x.Content == textName);

            Model.Texts.Remove(text);

            await Model.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Delete a user file
        /// </summary>
        /// <param name="textName">text Name</param>
        /// <returns>Deletion result</returns>
        [HttpPost("DeleteFile")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
            var user = Model.Users.FirstOrDefault(x => x.UserId == userId);
            var file = Model.Files.FirstOrDefault(x => x.Name == fileName);

            Model.Files.Remove(file);

            await Model.SaveChangesAsync();
            return Ok();
        }
    }
}
