using System;

namespace Play.Inventory.Service.Dtos
{
    public record GrantItemsDto(Guid UserId, Guid CatlogItemId, int Qty);
    //update this record to include name and description form Catlog 
    //public record InventoryItemDto(Guid CatlogItemId, int Qty, DateTimeOffset Acquireddate);
    public record InventoryItemDto(Guid CatlogItemId, string Name, string Description, int Qty, DateTimeOffset Acquireddate);
    //this is what the inventory app will use to get data from Catlog Service
    public record CatalogItemDto(Guid Id, string Name, string Description);

}