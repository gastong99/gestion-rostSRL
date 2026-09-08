using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class StockTransfersController : Controller
    {
        private readonly IStockTransferService _stockTransferService;

        public StockTransfersController(IStockTransferService stockTransferService)
        {
            _stockTransferService = stockTransferService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int itemId, int sourceWarehouseId, int destinationWarehouseId, decimal quantity)
        {
            try
            {
                await _stockTransferService.TransferAsync(itemId, sourceWarehouseId, destinationWarehouseId, quantity);

                TempData["Success"] = "Transferencia de stock realizada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index", "Stocks");
        }
    }
}