using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazorWebApp.Services;
using BlazorWebApp.Models;

namespace BlazorWebApp.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        public CommentsController(ICommentService service)
            => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<CommentDto>>> List()
        {
            var comments = await _service.ListAsync();
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest();

            var dto = await _service.AddAsync(text);
            return Ok(dto);
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> SetRead(Guid id, [FromQuery] bool isRead = true)
        {
            await _service.SetReadStatusAsync(id, isRead);
            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> UnreadCount()
        {
            var count = await _service.CountUnreadAsync();
            return Ok(count);
        }
    }
}
