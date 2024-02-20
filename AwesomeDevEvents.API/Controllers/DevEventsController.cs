using AutoMapper;
using AwesomeDevEvents.API.Entities;
using AwesomeDevEvents.API.Models;
using AwesomeDevEvents.API.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AwesomeDevEvents.API.Controllers {
    [Route("api/dev-events/v1")]
    [ApiController]
    public class DevEventsController : ControllerBase {

        private readonly DevEventsDbContext _dbContext;
        private readonly IMapper _mapper;

        public DevEventsController(DevEventsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

         /// <summary>
         /// Obter todos os eventos
         /// </summary>
         /// <returns>Coleção de eventos</returns>
         /// <response code="200">Sucesso</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll() {
            var devEvents = _dbContext.DevEvents.Where(d => !d.IsDeleted).ToList();
            var viewModel = _mapper.Map<List<DevEventViewModel>>(devEvents);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obter um evento especifico apartir de um identificador
        /// </summary>
        /// <param name="id">Identificador do evento</param>
        /// <returns>Dados de um evento</returns>
        /// <response code="200">Sucesso</response>
        /// <response code="404">Não encontrado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id) {
            var devEvents = _dbContext.DevEvents
                .Include(de =>  de.Speakers)
                .SingleOrDefault(d => d.Id == id);
            if (devEvents == null) return NotFound();

            var viewModel = _mapper.Map<DevEventByIdViewModel>(devEvents);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cadastrar um novo evento
        /// </summary>
        /// <remarks>
        /// Objeto JSON
        /// </remarks>
        /// <param name="inputModel">Dados do evento</param>
        /// <returns>Objeto recém criado</returns>
        /// <response code="201">Sucesso</response>

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Post(DevEventInputModel inputModel) {

            var devEvent = _mapper.Map<DevEvent>(inputModel);
            _dbContext.DevEvents.Add(devEvent);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetById), new {id =  devEvent.Id}, devEvent);

        }
        
        /// <summary>
        /// Atualizar um evento
        /// </summary>
        /// <remarks>
        /// Objeto JSON
        /// </remarks>
        /// <param name="id">Identificador de um evento</param>
        /// <param name="inputEvent">Dados do evento</param>
        /// <returns>Nada</returns>
        /// <responde code="204">Sucesso</responde>
        /// <response code="404">Não encontrado</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, DevEventInputModel inputEvent) {

            var devEvent = _dbContext.DevEvents.SingleOrDefault(d => d.Id == id);
            if (devEvent == null) return NotFound();

            devEvent.Update(inputEvent.Title, inputEvent.Description, inputEvent.StartDate, inputEvent.EndDate);
            _dbContext.DevEvents.Update(devEvent);
            _dbContext.SaveChanges();

            return NoContent();

        }

        /// <summary>
        /// Deletar um evento
        /// </summary>
        /// <param name="id">Identificador do evento</param>
        /// <returns>Nada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="404">Não encontrado</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id) {
            var devEvents = _dbContext.DevEvents.SingleOrDefault(d => d.Id == id);
            if (devEvents == null) return NotFound();
            devEvents.Delete();
            _dbContext.SaveChanges();
            return NoContent();

        }

        /// <summary>
        /// Cadastrar palestrante
        /// </summary>
        /// <param name="id">Identificador do palestrante</param>
        /// <param name="inputModel">Dados do palestrante</param>
        /// <returns>Nada</returns>
        /// <response code="204">Sucesso</response>
        /// <response code="404">Evento Não encontrado</response>
        [HttpPost("{id}/speakers")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddSpeaker(Guid id, DevEventSpeakerInputModel inputModel) {

            var speaker = _mapper.Map<DevEventSpeaker>(inputModel);
            speaker.DevEventId = id;
            var devEvent = _dbContext.DevEvents.Any(d => d.Id == id);
            if (!devEvent) return NotFound();
            _dbContext.DevEventSpeaker.Add(speaker);
            _dbContext.SaveChanges();
            return NoContent();

        }


    }
}
