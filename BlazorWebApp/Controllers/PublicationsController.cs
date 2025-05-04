using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazorWebApp.Models;
using BlazorWebApp.Services;

namespace BlazorWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicationsController : ControllerBase
    {
        private readonly IPublicationService _svc;
        public PublicationsController(IPublicationService svc) => _svc = svc;

        [HttpGet]
        public Task<List<PublicationReadDto>> List() => _svc.ListAsync();

        [HttpGet("{id:guid}")]
        public Task<PublicationReadDto?> Get(Guid id) => _svc.GetAsync(id);

        [HttpPost]
        public Task<PublicationReadDto> Create([FromBody] PublicationWriteDto dto) =>
            _svc.CreateAsync(dto);

        [HttpPost("{id:guid}/publish")]
        public Task Publish(Guid id) => _svc.PublishAsync(id);
    }
}
