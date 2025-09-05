using GerenciamentoDePedidos.Application.DTOs;
using GerenciamentoDePedidos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GerenciamentoDePedidos.Presentation.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerService _service;

        public CustomersController(CustomerService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var result = await _service.SearchAsync(search);
                    return View(result);
                }

                var customers = await _service.GetAllAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao carregar a lista de clientes.");
                return View(Enumerable.Empty<CustomerDto>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                await _service.AddAsync(new CustomerDto
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone
                });

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro inesperado ao criar o cliente.");
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _service.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao buscar cliente para edição.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                await _service.UpdateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro inesperado ao atualizar o cliente.");
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _service.GetByIdAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao buscar cliente para exclusão.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return BadRequest("Erro ao excluir cliente.");
            }
        }
    }
}