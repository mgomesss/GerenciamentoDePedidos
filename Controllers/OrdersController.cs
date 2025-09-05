using GerenciamentoDePedidos.Application.DTOs;
using GerenciamentoDePedidos.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GerenciamentoDePedidos.Presentation.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderService _service;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;

        public OrdersController(OrderService service, CustomerService customerService, ProductService productService)
        {
            _service = service;
            _customerService = customerService;
            _productService = productService;
        }

        public async Task<IActionResult> Index(string searchCustomer, string status)
        {
            var orders = await _service.GetAllAsync();
            if (!string.IsNullOrEmpty(searchCustomer))
            {
                orders = orders.Where(o => o.CustomerName.Contains(searchCustomer, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }
            ViewBag.SearchCustomer = searchCustomer;
            ViewBag.Status = status;
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "Id", "Name");
            ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
            return View(new OrderCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateDto model)
        {
            if (!ModelState.IsValid || model.CustomerId == 0 || model.Products == null || !model.Products.Any() || model.Products.Any(p => p.ProductId == 0 || p.Quantity <= 0))
            {
                ModelState.AddModelError("", "Selecione um cliente válido e pelo menos um produto com quantidade válida.");
                ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "Id", "Name");
                ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
                return View(model);
            }

            try
            {
                var items = model.Products.Select(p => (p.ProductId, p.Quantity)).ToList();
                await _service.CreateAsync(model.CustomerId, items);
                TempData["SuccessMessage"] = "Pedido criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "Id", "Name");
                ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
                return View(model);
            }
        }

        public async Task<IActionResult> EditStatus(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Statuses = new SelectList(new[]
            {
                new { Value = "Novo", Text = "Novo" },
                new { Value = "Processando", Text = "Processando" },
                new { Value = "Finalizado", Text = "Finalizado" }
            }, "Value", "Text", order.Status);

            return View(new OrderStatusDto { Id = order.Id, Status = order.Status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(OrderStatusDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Statuses = new SelectList(new[]
                {
                    new { Value = "Novo", Text = "Novo" },
                    new { Value = "Processando", Text = "Processando" },
                    new { Value = "Finalizado", Text = "Finalizado" }
                }, "Value", "Text", model.Status);
                ModelState.AddModelError("", "Selecione um status válido.");
                return View(model);
            }

            try
            {
                await _service.UpdateStatusAsync(model.Id, model.Status);
                TempData["SuccessMessage"] = $"Status do pedido #{model.Id} atualizado para '{model.Status}' com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Statuses = new SelectList(new[]
                {
                    new { Value = "Novo", Text = "Novo" },
                    new { Value = "Processando", Text = "Processando" },
                    new { Value = "Finalizado", Text = "Finalizado" }
                }, "Value", "Text", model.Status);
                return View(model);
            }
        }
    }
}