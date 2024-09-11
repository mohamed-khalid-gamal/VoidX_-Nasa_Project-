using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nasa.Data;
using Nasa.DTO;
using Nasa.Models;

namespace Nasa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController(ApplicationDbContext _db) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SendMessage(MessageDTO message)
        {
            if (message != null)
            {
                if (ModelState.IsValid)
                {
                    
                    _db.messages.Add(new Message() { Content  = message.Content , Email = message.Email , Name = message.Name , Subject = message.Subject});
                    _db.SaveChanges();
                    return Created();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else
            {
                
            return BadRequest("No Message To Send");
            }
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles="Admin")]
        public IActionResult GetAllMessages()
        {
            return Ok(_db.messages.ToList());
        }
        [HttpDelete("Id={id:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteMessage(int id)
        {
            if (id <=0)
            {
                return BadRequest("invalid id");
            }
            var message = _db.messages.SingleOrDefault(m => m.Id == id);
            if (message == null) {
                return NotFound("No message with this id");
            }
            _db.messages.Remove(message);
            _db.SaveChanges();
            return Ok("Deleted Succesfully");
        }
    }
}
