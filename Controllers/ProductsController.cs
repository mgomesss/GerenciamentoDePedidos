using GerenciamentoDePedidos.Application.DTOs;
using GerenciamentoDePedidos.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GerenciamentoDePedidos.Presentation.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
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

                var products = await _service.GetAllAsync();
                return View(products);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao carregar produtos.");
                return View(Enumerable.Empty<ProductDto>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                await _service.AddAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro inesperado ao criar produto.");
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var p = await _service.GetByIdAsync(id);

                if (p == null)
                {
                    return NotFound();
                }

                return View(p);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao buscar produto para edição.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductDto dto)
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
                ModelState.AddModelError("", "Erro inesperado ao atualizar produto.");
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var p = await _service.GetByIdAsync(id);

                if (p == null)
                {
                    return NotFound();
                }

                return View(p);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao buscar produto para exclusão.");
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
                return BadRequest("Erro ao excluir produto.");
            }
        }
    }
}
