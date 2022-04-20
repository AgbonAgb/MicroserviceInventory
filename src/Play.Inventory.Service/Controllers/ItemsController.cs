using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        //Making use of our generic repo in Nuget
        private readonly IRepository<InventoryItem> _itemRepository;
        private readonly CatLogClient _catalogclient;
        //Do the injection
        public ItemsController(IRepository<InventoryItem> itemRepository, CatLogClient catalogclient)
        {
            _itemRepository = itemRepository;
            _catalogclient = catalogclient;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> getAsync(Guid UserId)
        {
            if (UserId == Guid.Empty)
            {
                return BadRequest();
            }
            else
            {
                //Note UserId is not in the default _itemRepository.GetAllAsync, but because we tied it 
                //the readonly repository to it, it uses the InventoryItem Entity
                //var items = (await _itemRepository.GetAllAsync(itm => itm.UserId == UserId))
                //.Select(itm => itm.AsDto());
                //return Ok(items);

                //new
                var catlogItems = await _catalogclient.GetCataLogItemAsync();
                var InventoryitemsEntity = await _itemRepository.GetAllAsync(itm => itm.UserId == UserId);

                //Combine the two info
                //InventoryitemsEntity is a subset of catlogItems
                var inventoryItemDtos = InventoryitemsEntity.Select(invenoryitem =>
                {
                    var catlogitem = catlogItems.Single(catlogitem => catlogitem.Id == invenoryitem.CataLogItemId);
                    return invenoryitem.AsDto(catlogitem.Name, catlogitem.Description);
                });

                 return Ok(inventoryItemDtos);

            }
        }
        [HttpPost]
        public async Task<ActionResult> AddInventory(GrantItemsDto grantItemDto)
        {
            var inventoryiem = await _itemRepository.GetAsync(x => x.UserId == grantItemDto.UserId && x.CataLogItemId == grantItemDto.CatlogItemId);

            if (inventoryiem == null)
            {
                inventoryiem = new InventoryItem
                {
                    CataLogItemId = grantItemDto.CatlogItemId,
                    Qty = grantItemDto.Qty,
                    UserId = grantItemDto.UserId,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await _itemRepository.CreateAsync(inventoryiem);
            }
            else
            {
                inventoryiem.Qty += grantItemDto.Qty;
                await _itemRepository.UpdateDb(inventoryiem);

            }

            return Ok();
        }

    }
}