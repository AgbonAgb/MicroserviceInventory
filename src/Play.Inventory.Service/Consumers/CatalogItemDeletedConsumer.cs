using System.Threading.Tasks;
using MassTransit;
using Play.CataLog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> _Repository;
        public CatalogItemDeletedConsumer(IRepository<CatalogItem> Repository)
        {
            _Repository = Repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var msg = context.Message;
            //check if the item has been consumed before
            var msgexisted = await _Repository.GetAsync(msg.ItemId);
            if (msgexisted == null)
            {
               return;
            }
           await _Repository.RemoveAsync(msg.ItemId);

        }
    }

}