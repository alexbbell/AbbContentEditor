using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankOperationsController : ControllerBase
    {
        private readonly IRepository<BankOperation> _repository;
        private readonly ILogger<BlogsController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public BankOperationsController(IRepository<BankOperation> repository, ILogger<BlogsController> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _logger = logger;
        }
        // GET: api/<BankOperationsController>
        [HttpGet]
        public IActionResult GetBankOperations()
        {
            var request = _repository.GetAll();
            BankOperationReponse bankOperationReponse = new BankOperationReponse
            {
                Total = request.Count(),
                BankOperations = request.ToList()
            };

            return Ok(bankOperationReponse);
        }

        // GET api/<BankOperationsController>/5
        [HttpGet("{id}")]
        public Task<BankOperation> GetBankOperation(int id)
        {
            var res = _repository.GetByIdAsync(id);
            return res;
        }

        // POST api/<BankOperationsController>
        [HttpPost]
        public async Task<ActionResult<BankOperation>> PostBankOperation(BankOperation bankOperation)
        {
            try
            {
                
                _unitOfWork.bankOperationRepository.AddAsync(bankOperation);
                await _unitOfWork.Commit();
                _logger.LogInformation($"Bank operation is added.");
                //return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
                return CreatedAtAction(nameof(GetBankOperation), new { id = bankOperation.Id }, bankOperation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error on adding the bank operation. {ex}");
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<BankOperationsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBankOperation(int id, BankOperation bankOperation)
        {
            if (id != bankOperation.Id)
            {
                _logger.LogError($"Error on updating the bank operation. Bank operation {id} not found");

                return BadRequest();
            }
            try
            {
                _unitOfWork.bankOperationRepository.UpdateAsync(bankOperation);
                _unitOfWork.Commit();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (await _unitOfWork.bankOperationRepository.GetByIdAsync(id) == null)
                {
                    _logger.LogError($"Error on updating the  bank operation. Error on save {id}. {ex}");
                    return NotFound();
                }
                else
                {
                    _logger.LogError($"Error on updating the  bank operation. {ex}");
                    return BadRequest(ex);
                }
            }
        }

        // DELETE api/<BankOperationsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankOperation(int id)
        {

            var bankOperation = await _unitOfWork.bankOperationRepository.GetByIdAsync(id);
            if (bankOperation == null) return NotFound();

            try
            {
                _ = _unitOfWork.bankOperationRepository.DeleteAsync(bankOperation);
                _ = _unitOfWork.Commit();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
