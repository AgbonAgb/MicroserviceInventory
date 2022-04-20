using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        //Map Inventory Entity <InventoryItem> into  DTO
        public static InventoryItemDto AsDto(this InventoryItem item, string Name, string Description)
        {
            // return new InventoryItemDto(item.CataLogItemId, item.Qty, item.AcquiredDate);
            //add string Name, string Description to confrm with the Dto that has been updated.
            return new InventoryItemDto(item.CataLogItemId, Name, Description, item.Qty, item.AcquiredDate);
        }
    }
}