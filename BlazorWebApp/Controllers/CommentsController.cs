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
        public async Task<IActionResult> Create([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest();

            await _service.AddAsync(text);
            return Ok();
        }
    }
}
